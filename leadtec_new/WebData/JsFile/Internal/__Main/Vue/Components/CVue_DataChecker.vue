<template>
  <div id="mainPage">
    <h5>收支資料檢誤</h5>
    <selector
      :isDataCheckBtn="true"
      :filterModel="selectorModel"
      @search="searchEvent"
    ></selector>

    <div id="tableData">
      <div class="w-100 d-inline-flex justify-content-center">
        <div class="float-sm-left">
          <label>檢誤筆數: {{ items.length }} 筆</label>
        </div>
        <div>
          <label>每頁顯示筆數</label>
          <b-form-input
            v-model="perPage"
            type="number"
            size="sm"
            class="d-inline-block w-25"
          ></b-form-input>
        </div>
        <b-button
          variant="info"
          @click="exportResult"
          class="mx-1 float-sm-right"
          :disabled="items.length === 0"
          size="sm"
        >
          匯出
        </b-button>
      </div>
      <b-table
        ref="domDatatable"
        :items="items"
        :fields="fields"
        :per-page="perPage"
        :current-page="currentPage"
        hover
        small
        head-variant="dark"
        class="my-2"
      >
        <a
          href="javascript:;"
          slot="[fam_no]"
          slot-scope="data"
          @click="showDetail(data.item.ie_day, data.item.fam_no)"
        >
          {{ data.item.fam_no }}
        </a>
        <template slot="[action]" slot-scope="data">
          <a href="javascript:;" @click="openDeleteDialog(data.item)">
            <img width="24px" :src="$options.imgSrc.trash" />
          </a>
          <a href="javascript:;" @click="editItem(data.item)">
            <img width="24px" :src="$options.imgSrc.edit" />
          </a>
        </template>
        <span slot="[state]" slot-scope="data">
          {{ stateToString(data.item.state) }}
        </span>
      </b-table>
      <b-pagination
        v-model="currentPage"
        :total-rows="items.length"
        :per-page="perPage"
        size="sm"
        class="justify-content-center"
      ></b-pagination>
    </div>

    <b-modal ref="domModal" size="xl" title="明細維護" hide-footer>
      <detailed-view
        :queryObject="detailedQueryObj"
        :data="detailedData"
        :remark="passRemark"
        @save="onSaveEvent"
      ></detailed-view>
    </b-modal>
  </div>
</template>

<script>
import { dataCheckerModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import DetailedView from './CVue_DetailedView.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'DataChecker',
  components: {
    Selector,
    DetailedView,
  },
  props: {},
  data() {
    return {
      selectorModel: dataCheckerModel,
      queryObject: {},
      detailedQueryObj: {},
      detailedData: [],
      passRemark: '',

      // for table
      items: [],
      currentPage: 1,
      perPage: 20,
    };
  },
  methods: {
    async searchEvent(filterObject) {
      const { checkType, checker, port, date, checktime } = filterObject;
      this.queryObject = {};

      // add date
      if (date.year !== 0 && date.month !== 0) {
        this.queryObject.Year = date.year;
        this.queryObject.Month = date.month;
      }

      // add port
      if (
        port.end.length !== 0 &&
        port.start.length !== 0 &&
        port.end >= port.start
      ) {
        this.queryObject.FamNoStart = port.start;
        this.queryObject.FamNoEnd = port.end;
      }

      if (checker && checker.id.length !== 0) {
        this.queryObject.CheckNo = checker.id;
      }

      if (checkType.code >= 0) {
        this.queryObject.CheckType = checkType.code;
      }

      if (checktime.num.length > 0) {
        this.queryObject.ChechTime = checktime.num;
      }

      await this.queryCheckData(this.queryObject);
    },
    async queryCheckData(queryObject) {
      this.items = [];
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.dataChecker,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        return;
      }

      this.items = resObject.data || [];
    },
    exportResult() {
      const checkeddData = this.items;

      // create txt data
      const csvData = [
        [
          `年`,
          `月`,
          `日`,
          `戶號`,
          `科目代碼`,
          `科目名稱`,
          `金額`,
          `金額上限`,
          `金額下限`,
          `檢誤代碼`,
          `說明`,
        ],
        ...checkeddData.map(obj => {
          const {
            ie_year,
            ie_mon,
            ie_day,
            fam_no,
            code_no,
            code_name,
            code_amt,
            upp_lim,
            low_lim,
            chk_no,
            chk_desc,
          } = obj;
          return [
            ie_year,
            ie_mon,
            ie_day,
            fam_no,
            code_no,
            code_name,
            code_amt,
            upp_lim,
            low_lim,
            chk_no,
            chk_desc,
          ];
        }),
      ];
      const csvDataString = csvData.map(col => col.join(`,`)).join('\n');
      const encodedUri = URL.createObjectURL(
        new Blob([`\uFEFF${csvDataString}`], {
          type: `text/csv;charset=utf-8;`,
        })
      );

      // create link
      const link = document.createElement(`a`);
      link.setAttribute(`href`, encodedUri);
      link.setAttribute(`download`, `data_ckeck_result.csv`);
      document.body.appendChild(link);
      link.click();
    },
    showDetail(day, famNo) {
      const { Year, Month } = this.queryObject;
      this.detailedQueryObj = {
        Year,
        Month,
        FamNo: famNo,
        Day: parseInt(day, 10),
      };

      this.$refs.domModal.show();
    },
    async onSaveEvent({ items, remark, totalCost, queryObject }) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      const { Year, Month, Day, FamNo } = queryObject;
      const filteredItems = resObject.data.CoExpD.filter(
        obj =>
          obj.ie_year === Year &&
          obj.ie_mon === Month &&
          obj.ie_day === Day &&
          obj.fam_no === FamNo
      );

      const updateItems = items.filter(obj => obj.item_no !== undefined);
      const insertItems = items.filter(obj => !obj.item_no);
      const deleteItems = filteredItems.filter(
        obj =>
          updateItems.map(obj2 => obj2.item_no).includes(obj.item_no) === false
      );

      let isSuccess = false;
      if (deleteItems.length > 0) {
        await this.deleteItems(deleteItems);
      }

      if (updateItems.length > 0 || insertItems.length > 0) {
        isSuccess = await this.saveItems(
          updateItems,
          insertItems,
          remark,
          totalCost
        );
      }

      if (isSuccess) {
        alert('修改成功');
      } else {
        alert('修改失敗');
      }

      this.$refs.domModal.hide();
    },
    async saveItems(updateItems, insertItems, remark, totalCost) {
      let famNo;
      let ieYear;
      let ieMon;
      let ieDay;
      if (updateItems.length === 0) {
        famNo = insertItems[0].fam_no;
        ieYear = insertItems[0].ie_year;
        ieMon = insertItems[0].ie_mon;
        ieDay = insertItems[0].ie_day;
      } else {
        famNo = updateItems[0].fam_no;
        ieYear = updateItems[0].ie_year;
        ieMon = updateItems[0].ie_mon;
        ieDay = updateItems[0].ie_day;
      }

      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `WRITE`,
          UpdateItems: JSON.stringify(updateItems),
          InsertItems: JSON.stringify(insertItems),
          FamNo: famNo,
          Year: ieYear,
          Month: ieMon,
          Day: ieDay,
          TotalCost: totalCost,
          Remark: remark,
        }
      );

      return resObject.status === this.mixinBackendErrorCode.success;
    },
    async deleteItems(items) {
      const tempItems = items.length > 0 ? items : this.selected;
      if (!tempItems || tempItems.length === 0) {
        return;
      }

      const deleteItemNoAry = tempItems.map(obj => obj.item_no);

      await this.mixinCallBackService(this.mixinBackendService.detatiledData, {
        Action: `DELETE`,
        ItemArray: JSON.stringify(deleteItemNoAry),
        FamNo: tempItems[0].fam_no,
      });
    },
  },
  created() {},
  mounted() {},
  computed: {
    fields() {
      return [
        { key: `ie_year`, label: `年` },
        { key: `ie_mon`, label: `月` },
        { key: `ie_day`, label: `日` },
        { key: `fam_no`, label: `戶號` },
        { key: `code_no`, label: `科目代碼` },
        { key: `code_name`, label: `科目名稱` },
        { key: `code_amt`, label: `金額` },
        { key: `upp_lim`, label: `金額上限` },
        { key: `low_lim`, label: `金額下限` },
        { key: `chk_no`, label: `檢誤代碼` },
        { key: `chk_desc`, label: `說明` },
      ];
    },
  },
  watch: {},
  /* eslint-disable no-undef, no-param-reassign, camelcase */
};
</script>

<style scoped>
#mainPage {
  text-align: center;
}
#tableData {
  width: 85%;
  margin: auto;
}
</style>
