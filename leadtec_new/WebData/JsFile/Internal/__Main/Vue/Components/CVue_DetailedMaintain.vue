<template>
  <div id="mainPage">
    <h5>收支明細維護</h5>
    <selector
      v-if="isShowFilter"
      :filterModel="selectorModel"
      :isDetailed="true"
      @search="searchEvent"
      @additem="openDetailedView"
    ></selector>
    <b v-else class="my-2">{{ title }}</b>

    <!-- filtered data -->
    <div id="tableData" v-if="items.length > 0">
      <b-button-group class="my-3 float-sm-left" size="sm">
        <b-button @click="selectAllRows">全選</b-button>
        <b-button @click="clearSelected">取消全選</b-button>
      </b-button-group>
      <div class="w-25 d-inline-block justify-content-center">
        <label class="d-inline-block">每頁顯示筆數</label>
        <b-form-input
          v-model="perPage"
          type="number"
          size="sm"
          class="d-inline-block"
        ></b-form-input>
      </div>
      <b-button-group class="my-3 float-sm-right" size="sm">
        <b-button
          variant="info"
          :disabled="!selected.length > 0"
          @click="deleteItems"
        >
          刪除
        </b-button>
      </b-button-group>

      <b-table
        ref="domDatatable"
        :busy="isBusy"
        :items="items"
        :fields="fields"
        :per-page="perPage"
        :current-page="currentPage"
        selectable
        select-mode="multi"
        selected-variant="info"
        @row-selected="onRowSelected"
        hover
        small
        head-variant="dark"
      >
        <div slot="table-busy" class="text-center text-danger my-2">
          <b-spinner class="align-middle"></b-spinner>
          <strong>Loading...</strong>
        </div>
        <div slot="empty" class="text-center text-danger my-2">
          <strong>資料不存在</strong>
        </div>
        <template slot="[code_name]" slot-scope="{ item }">
          <div style="text-align: left;">{{ item.code_name }}</div>
        </template>
        <template slot="[selected]" slot-scope="{ rowSelected, index }">
          <b-form-checkbox
            v-model="rowSelected"
            @change="selectOneItem(index)"
          ></b-form-checkbox>
        </template>
        <template slot="[place]" slot-scope="{ item }">
          <div style="text-align: left;">{{ placeName(item.place) }}</div>
        </template>
        <template slot="[exp_amt]" slot-scope="{ item }">
          <div>{{ totalCost(item) }}</div>
        </template>
        <template slot="[edit]" slot-scope="{ item }">
          <a href="javascript:;" @click="openDetailedView(item)">編輯</a>
        </template>
      </b-table>

      <b-pagination
        v-model="currentPage"
        :total-rows="items.length"
        :per-page="perPage"
        size="sm"
        class="justify-content-center"
      ></b-pagination>
    </div>

    <span v-else>{{ hintText }}</span>

    <!-- add data -->
    <b-modal ref="domModal" size="xl" title="明細維護" hide-footer>
      <detailed-view
        :queryObject="addQueryObject"
        :data="detailedData"
        :remark="passRemark"
        @save="onSaveEvent"
      ></detailed-view>
    </b-modal>
  </div>
</template>

<script>
import { mapState } from 'vuex';
import { detailedModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import DetailedView from './CVue_DetailedView.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'DetailedMaintain',
  components: {
    Selector,
    DetailedView,
  },
  props: {
    isShowFilter: {
      type: Boolean,
      default: true,
    },
    inputedQueryObj: {
      type: Object,
    },
  },
  data() {
    return {
      // for selector
      selectorModel: detailedModel,
      hintText: `請先查詢資料`,
      queryObject: {},
      addQueryObject: {},

      // for select
      isSelectAll: false,
      selectedIndex: [],

      // for table
      fields: [],
      items: [],
      coExpMitems: [],
      currentPage: 1,
      perPage: 20,
      selected: [],
      isBusy: false,

      // for edit, and new one
      detailedData: [],
      passRemark: ``,
    };
  },
  methods: {
    async searchEvent(filterObj) {
      const { date, duration, port, subjectCode, subjectName } = filterObj;
      this.queryObject = {};

      // add date
      if (date.year !== 0 && date.month !== 0) {
        this.queryObject.Year = date.year;
        this.queryObject.Month = date.month;
      }

      // add duration
      if (
        duration.start.length > 0 &&
        duration.end.length > 0 &&
        duration.end >= duration.start
      ) {
        this.queryObject.DurationStart = duration.start;
        this.queryObject.DurationEnd = duration.end;
      }

      // add port
      if (port.num > 0) {
        this.queryObject.FamNo = port.num;
      }

      // add subjectCode
      if (subjectCode.code_no > 0) {
        this.queryObject.CodeNo = subjectCode.code_no;
      }

      // add subject name
      if (subjectName.code_name.length > 0) {
        this.queryObject.CodeName = subjectName.code_name;
      }

      await this.queryDetailedData(this.queryObject);
    },
    onRowSelected(items) {
      this.selected = items;
    },
    selectAllRows() {
      this.$refs.domDatatable.selectAllRows();
      this.isSelectAll = true;
    },
    clearSelected() {
      this.$refs.domDatatable.clearSelected();
      this.isSelectAll = false;
    },
    selectOneItem(index) {
      const isSelected = this.$refs.domDatatable.isRowSelected(index);
      if (isSelected) {
        this.$refs.domDatatable.unselectRow(index);
        return;
      }

      this.$refs.domDatatable.selectRow(index);
    },
    openDetailedView(dataObj) {
      if (dataObj) {
        this.detailedData = JSON.parse(
          JSON.stringify(
            this.items.filter(item => item.ie_day === dataObj.ie_day)
          )
        );
        const expObject = this.coExpMitems.find(
          obj => obj.ie_day === dataObj.ie_day
        );
        this.passRemark = expObject === undefined ? '' : expObject.day_rem;
      } else {
        const { Year, Month, FamNo } = this.queryObject;
        this.detailedData = [];
        this.addQueryObject = {
          Year,
          Month,
          FamNo,
        };
      }

      this.$refs.domModal.show();
    },
    async queryDetailedData(queryObject) {
      this.isBusy = true;
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        this.coExpMitems = resObject.data.CoExpM || [];
        this.items = resObject.data.CoExpD || [];

        if (this.items.length === 0) {
          this.hintText = `無資料`;
        } else {
          // sort
          this.items.sort((obj1, obj2) => {
            if (obj1.ie_day > obj2.ie_day) {
              return 1;
            }

            if (obj1.ie_day < obj2.ie_day) {
              return -1;
            }

            return 0;
          });
        }
      } else {
        this.hintText = `查詢錯誤發生，請確認有輸入必要參數`;
      }

      this.isBusy = false;
      return resObject;
    },
    async onSaveEvent({ items, remark, totalCost, queryObject }) {
      const { Year, Month, Day, FamNo } = queryObject;
      const filteredItems = this.items.filter(
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

      if (deleteItems.length > 0) {
        await this.deleteItems(deleteItems);
      }

      if (updateItems.length > 0 || insertItems.length > 0) {
        await this.saveItems(updateItems, insertItems, remark, totalCost);
      }

      // reload
      await this.queryDetailedData(this.queryObject);

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

      if (resObject.status === this.mixinBackendErrorCode.success) {
        for (let i = 0; i < updateItems.length; i++) {
          const itemIdx = this.items.findIndex(
            obj => obj.item_no === updateItems[i].item_no
          );
          this.$set(this.items, itemIdx, updateItems[i]);
        }
        this.items = [...insertItems, ...this.items];
      }
    },
    async deleteItems(items) {
      const tempItems = items.length > 0 ? items : this.selected;
      if (!tempItems || tempItems.length === 0) {
        return;
      }

      const deleteItemNoAry = tempItems.map(obj => obj.item_no);

      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `DELETE`,
          ItemArray: JSON.stringify(deleteItemNoAry),
          FamNo: tempItems[0].fam_no,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        // delete item in local var
        for (let i = 0; i < tempItems.length; i++) {
          const itemIdx = this.items.findIndex(
            obj => obj.item_no === tempItems[i].item_no
          );
          this.$delete(this.items, itemIdx);
        }
      }
    },
  },
  created() {
    this.fields = [
      { key: `selected`, label: `勾選` },
      { key: `ie_day`, label: `日期` },
      { key: `exp_amt`, label: `支出合計` },
      { key: `place`, label: `購買地點` },
      { key: `code_amt`, label: `金額` },
      { key: `code_no`, label: `科目代碼` },
      { key: `code_name`, label: `科目名稱` },
      { key: `edit`, label: `` },
    ];
  },
  async mounted() {
    if (this.inputedQueryObj) {
      await this.queryDetailedData(this.inputedQueryObj);
    }
  },
  computed: {
    ...mapState([`paramArray`, `subjectArray`]),
    placeName() {
      return placeNo => {
        if (!placeNo || placeNo === 0) {
          return ``;
        }

        if (this.paramArray.length === 0) {
          return ``;
        }

        const paramObj = this.paramArray.find(
          obj => obj.par_typ === `A` && obj.par_no === placeNo
        );
        if (!paramObj) {
          return ``;
        }

        return `${paramObj.par_no} ${paramObj.par_name}`;
      };
    },
    title() {
      if (this.isShowFilter) {
        return ``;
      }

      const { Year, Month, FamNo } = this.inputedQueryObj;
      return `${Year}年${Month}月, 戶號:${FamNo}`;
    },
    totalCost() {
      return item => {
        const itemDay = item.ie_day;
        return this.items.reduce((cost, obj) => {
          if (obj.ie_day === itemDay) {
            cost += parseInt(obj.code_amt, 10);
          }

          return cost;
        }, 0);
      };
    },
  },
  watch: {
    currentPage: {
      handler() {
        this.$nextTick(() => {
          if (this.isSelectAll) {
            this.selectAllRows();
          }

          if (this.selectedIndex.length !== 0) {
            for (let i = 0; i < this.selectedIndex.length; i++) {
              this.$refs.domDatatable.selectRow(i);
            }
          }
        });
      },
    },
  },
  /* eslint-disable no-undef, no-param-reassign, camelcase */
};
</script>

<style scoped>
#mainPage {
  text-align: center;
}
#tableData {
  text-align: center;
  width: 85%;
  margin: auto;
}
</style>
