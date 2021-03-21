<template>
  <div id="mainPage">
    <h5>收支科目維護</h5>
    <selector :filterModel="selectorModel" @search="searchEvent"></selector>

    <!-- filtered data -->
    <div id="tableData">
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
          label="1234"
          class="d-inline-block"
        ></b-form-input>
      </div>
      <b-button-group class="my-3 float-sm-right" size="sm">
        <b-button
          variant="info"
          :disabled="selected.length > 0"
          @click="openDetailedView()"
        >
          新增
        </b-button>
        <b-button
          variant="info"
          :disabled="!selected.length > 0"
          @click="deleteItems"
        >
          刪除
        </b-button>
        <b-button
          variant="info"
          :disabled="!selected.length > 0"
          @click="exportCSV"
        >
          匯出資料
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
        <template slot="[selected]" slot-scope="{ rowSelected, index }">
          <b-form-checkbox
            v-model="rowSelected"
            @change="selectOneItem(index)"
          ></b-form-checkbox>
        </template>
        <template slot="[code_name]" slot-scope="{ item }">
          <div style="text-align: left;">{{ item.code_name }}</div>
        </template>
        <template slot="[place]" slot-scope="{ item }">
          <span>{{ placeName(item.place) }}</span>
        </template>
        <template slot="[stop_fg]" slot-scope="{ item }">
          <span v-if="item.stop_fg === `Y`">是</span>
          <span v-else>否</span>
        </template>
        <template slot="[def_fg]" slot-scope="{ item }">
          <span v-if="item.def_fg === `Y`">是</span>
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

    <!-- add data -->
    <b-modal ref="domModal" size="lg" title="科目維護" hide-footer>
      <subject-view
        :subjectData="subjectData"
        @changed="onChange"
      ></subject-view>
    </b-modal>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import { subjectModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import SubjectView from './CVue_SubjectView.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'IncomeDataMaintain',
  components: {
    Selector,
    SubjectView,
  },
  props: {},
  data() {
    return {
      selectorModel: subjectModel,
      isSelectAll: false,
      selectedData: null,

      // subject model
      subjectData: {},

      fields: [],
      queryObject: null,
      currentPage: 1,
      perPage: 20,
      selected: [],
      isBusy: false,
    };
  },
  methods: {
    ...mapActions([
      `updateSubject`,
      `deleteSubjects`,
      `saveSubject`,
      `setSubjectDefaultName`,
    ]),
    async searchEvent(filterObject) {
      const { subjectCode, subjectName } = filterObject;
      // set default
      this.queryObject = {};
      if (subjectCode.code_no > 0) {
        this.queryObject.CodeNo = subjectCode.code_no;
      }

      if (subjectName.code_name.length > 0) {
        this.queryObject.CodeName = subjectName.code_name;
      }
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
    openDetailedView(dataObj) {
      if (dataObj) {
        this.selectedData = dataObj;
        this.subjectData = JSON.parse(JSON.stringify(dataObj));
      } else {
        this.selectedData = null;
        this.subjectData = {};
      }

      this.$refs.domModal.show();
    },
    selectOneItem(index) {
      const isSelected = this.$refs.domDatatable.isRowSelected(index);
      if (isSelected) {
        this.$refs.domDatatable.unselectRow(index);
        return;
      }

      this.$refs.domDatatable.selectRow(index);
    },
    async onChange({ subjectObj, isDefaultName }) {
      // set object keys
      if (!subjectObj.code1 || !subjectObj.code2) {
        subjectObj.code1 = subjectObj.code_no.toString().substring(0, 3);
        subjectObj.code2 = subjectObj.code_no.toString().substring(3);
      }

      // set desc
      subjectObj.code_desc = subjectObj.code_no + subjectObj.code_name;

      // save this subject
      if (this.selectedData === null) {
        await this.saveSubject(subjectObj);
      } else {
        await this.updateSubject({
          preSubjectObj: this.selectedData,
          newSubjectObj: subjectObj,
        });
      }

      // check is default name
      if (isDefaultName) {
        // do something
        await this.setSubjectDefaultName(subjectObj);
      }

      this.$refs.domModal.hide();
    },
    async deleteItems() {
      await this.deleteSubjects(
        this.selected.map(obj => {
          const { code_no, code_name } = obj;
          return {
            code_no,
            code_name,
          };
        })
      );

      this.clearSelected();
    },
    exportCSV() {
      // create csv data
      const filedName = [
        `科目代碼`,
        `科目名稱`,
        `金額上限`,
        `金額下限`,
        `購買地點`,
        `是否停用`,
      ];

      let exportData = this.selected;
      if (this.isSelectAll) {
        exportData = this.subjectArray;
      }

      const csvData = [
        filedName,
        ...exportData.map(obj => {
          const { code_no, code_name, upp_lim, low_lim, place, stop_fg } = obj;
          return [
            code_no,
            code_name,
            upp_lim,
            low_lim,
            this.placeName(place),
            stop_fg === `Y` ? `是` : `否`,
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
      link.setAttribute(`download`, `subject_export.csv`);
      document.body.appendChild(link);
      link.click();

      // clear
      this.clearSelected();
    },
  },
  created() {
    this.fields = [
      { key: `selected`, label: `勾選` },
      { key: `code_no`, label: `科目代碼` },
      { key: `code_name`, label: `科目名稱` },
      { key: `upp_lim`, label: `金額上限` },
      { key: `low_lim`, label: `金額下限` },
      { key: `upp_sys`, label: `系統金額上限` },
      { key: `low_sys`, label: `系統金額下限` },
      { key: `place`, label: `購買地點` },
      { key: `stop_fg`, label: `是否停用` },
      { key: `def_fg`, label: `預設名稱註記` },
      { key: `edit`, label: `` },
    ];
  },
  mounted() {},
  computed: {
    ...mapState([`subjectArray`, `paramArray`]),
    items() {
      if (!this.queryObject) {
        return [];
      }

      if (Object.keys(this.queryObject).length === 0) {
        return this.subjectArray;
      }

      return this.subjectArray.filter(obj => {
        if (this.queryObject.CodeName && this.queryObject.CodeName.length > 0) {
          const idx = obj.code_name.indexOf(this.queryObject.CodeName);
          if (idx !== -1) {
            return true;
          }
        }

        if (this.queryObject.CodeNo && this.queryObject.CodeNo.length > 0) {
          return obj.code_no.toString().startsWith(this.queryObject.CodeNo);
        }

        return false;
      });
    },
    placeName() {
      return placeNo => {
        if (!placeNo || placeNo === 0) {
          return ``;
        }

        return this.paramArray.find(
          obj => obj.par_typ === `A` && obj.par_no === placeNo
        ).par_name;
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
