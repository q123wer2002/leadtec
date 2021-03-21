<template>
  <div id="mainPage">
    <h5>收支資料維護</h5>
    <selector :filterModel="selectorModel" @search="searchEvent"></selector>

    <!-- filtered data -->
    <div id="tableData">
      <b-button-group class="my-3 float-sm-left" size="sm">
        <b-button @click="selectAllRows">全選</b-button>
        <b-button @click="clearSelected">取消全選</b-button>
      </b-button-group>
      <div class="d-inline-block justify-content-center" style="width:100px">
        <label class="d-inline-block">顯示筆數</label>
        <b-form-input
          v-model="perPage"
          type="number"
          size="sm"
          class="d-inline-block"
        ></b-form-input>
      </div>
      <b-button
        variant="info"
        :disabled="!enabledConfirm"
        @click="confirm(`2`)"
        class="my-3 mx-1 float-sm-right"
        size="sm"
      >
        登錄確認
      </b-button>
      <b-button-group class="my-3 float-sm-right" size="sm">
        <b-button
          variant="info"
          :disabled="!enabledReview"
          @click="confirm(`3`)"
        >
          審核確認
        </b-button>
        <b-button
          variant="info"
          :disabled="!isSelectedItems"
          @click="confirm(`1`)"
        >
          狀態回復
        </b-button>
        <b-button
          variant="info"
          :disabled="Object.keys(queryObject).length === 0"
          @click="openUploader"
        >
          收支匯入
        </b-button>
        <b-button
          variant="info"
          :disabled="!isSelectedItems"
          @click="exportTXT"
        >
          收支匯出
        </b-button>
        <b-button
          variant="info"
          :disabled="items.length === 0"
          @click="exportCompleteCSV"
          title="匯出整月份資料"
        >
          收支完整匯出
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
        <template slot="[state]" slot-scope="{ item }">
          <span>{{ statusToString(item.state) }}</span>
        </template>
        <template slot="[btnIncomeDetail]" slot-scope="{ item }">
          <a href="javascript:;" @click="showDetails(item.fam_no)">明細</a>
        </template>
      </b-table>

      <b-pagination
        v-model="currentPage"
        :total-rows="items.length"
        :per-page="perPage"
        size="sm"
        class="justify-content-center"
      ></b-pagination>

      <b-modal ref="domModal" size="xl" hide-footer>
        <detailed-maintain
          :isShowFilter="false"
          :inputedQueryObj="detailedQueryObj"
        ></detailed-maintain>
      </b-modal>
    </div>

    <div ref="domProgressBG" class="blackBG">
      <b-progress
        :max="pgbMax"
        height="3rem"
        style="width: 80%; margin: 20% auto 0 auto;"
      >
        <b-progress-bar :value="pgbValue">
          (請不要關閉) 進度:
          <strong>{{ progress }} ({{ pgbValue }} / {{ pgbMax }})</strong>
        </b-progress-bar>
      </b-progress>
    </div>

    <input
      type="file"
      accept="text/plain"
      ref="domFileInput"
      @change="importTXT"
      style="display: none;"
    />
  </div>
</template>

<script>
import { mapState } from 'vuex';
import { incomeDataModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import { statusMapToString } from '../DataModel/dataModel.js';
import DetailedMaintain from './CVue_DetailedMaintain.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase, no-await-in-loop */
  name: 'IncomeDataMaintain',
  components: {
    Selector,
    DetailedMaintain,
  },
  props: {},
  data() {
    return {
      selectorModel: incomeDataModel,
      detailedQueryObj: {},
      queryObject: {},
      isSelectAll: false,

      // progres bar
      pgbMax: 100,
      pgbValue: 0,

      fields: [],
      items: [],
      currentPage: 1,
      perPage: 20,
      selected: [],
      isBusy: false,
    };
  },
  methods: {
    async searchEvent(filterObject) {
      const { date, loginman, port, status, reviewman } = filterObject;
      this.queryObject = {};

      // add date
      if (date.year !== 0 && date.month !== 0) {
        this.queryObject.Year = date.year;
        this.queryObject.Month = date.month;
      }

      // add loginman
      if (loginman.id > 0) {
        this.queryObject.RecNo = loginman.id;
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

      if (reviewman && reviewman.id.length !== 0) {
        this.queryObject.AdiUser = reviewman.id;
      }

      if (status.code > 0) {
        this.queryObject.State = status.code;
      }

      await this.queryIncomeStateData(this.queryObject);
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
    async queryIncomeStateData(queryObject) {
      this.isBusy = true;
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.incomeData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        this.items = resObject.data || [];
      }
      this.isBusy = false;
      return resObject;
    },
    async confirm(confirmType) {
      const paramList = this.selected.map(famObj => {
        const { ie_year, ie_mon, fam_no } = famObj;
        return {
          ie_year,
          ie_mon,
          fam_no,
        };
      });

      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.incomeData,
        {
          Action: `CONFIRM`,
          ConfirmList: JSON.stringify(paramList),
          ConfirmType: confirmType,
        }
      );
      if (resObject.status === this.mixinBackendErrorCode.success) {
        this.selected.forEach(obj => {
          obj.state = confirmType;
        });

        this.clearSelected();
      }
    },
    openUploader() {
      this.$refs.domFileInput.value = ``;
      this.$refs.domFileInput.click();
    },
    importTXT(event) {
      const inputFiles = event.target;

      const reader = new FileReader();
      reader.onload = () => {
        const fileTxt = reader.result;
        if (fileTxt.length === 0) {
          alert(`選取的檔案沒有資料`);
          return;
        }

        this.showCheckBox().then(async value => {
          if (value === false) {
            return;
          }

          // parse data into income data
          const structuredData = fileTxt.split(`\n`).map(dataString => {
            return this.parseDataToStructure(dataString);
          });

          // divide by fam_no, and day
          const famDayObj = structuredData.reduce((tempObj, obj) => {
            if (!obj) {
              return tempObj;
            }

            const famNo = obj.fam_no;
            const day = obj.ie_day;

            if (!tempObj[famNo]) {
              tempObj[famNo] = {};
            }

            if (!tempObj[famNo][day]) {
              tempObj[famNo][day] = [];
            }

            tempObj[famNo][day].push(obj);
            return tempObj;
          }, {});

          this.showProgressBar();
          this.pgbMax = structuredData.length;
          let isSuccess = true;
          for (let i = 0; i < Object.values(famDayObj).length; i++) {
            const insertDays = Object.values(Object.values(famDayObj)[i]);
            for (let j = 0; j < insertDays.length; j++) {
              const insertItems = insertDays[j];

              // store data
              const isOk = await this.saveCoExp(insertItems);
              if (isOk === false) {
                isSuccess = false;
              }

              this.pgbValue += insertItems.length;
              if (this.pgbValue > this.pgbMax) {
                this.pgbValue = this.pgbMax;
              }
            }
          }

          alert(isSuccess ? `匯入成功` : `匯入失敗`);
          this.hideProgressBar();
        });
      };

      if (inputFiles.files.length === 0) {
        return;
      }

      reader.readAsText(inputFiles.files[0]);
    },
    parseDataToStructure(dataString) {
      if (dataString.length < 43) {
        return null;
      }

      const { Year, Month } = this.queryObject;
      return {
        ie_year: Year,
        ie_mon: Month,
        ie_day: dataString.substring(3, 5),
        fam_no: dataString.substring(5, 13),
        code_no: dataString.substring(13, 18),
        fam_cnt: dataString.substring(21, 22),
        job_cnt: dataString.substring(22, 23),
        job_no: dataString.substring(23, 25),
        job_typ_no: dataString.substring(25, 27),
        code_amt: parseInt(dataString.substring(27, 34), 10),
        item_no: dataString.substring(35, 41),
        place: dataString.substring(42, 43),
      };
    },
    showCheckBox() {
      const { Year, Month } = this.queryObject;
      return this.$bvModal.msgBoxConfirm(
        `您確定要匯入嗎？資料將會存入${Year}年${Month}月`,
        {
          title: '請確認',
          size: 'sm',
          buttonSize: 'sm',
          okVariant: 'danger',
          okTitle: '是',
          cancelTitle: '否',
          footerClass: 'p-2',
          hideHeaderClose: false,
          centered: true,
        }
      );
    },
    async exportTXT() {
      let detailedData = this.selected;
      if (this.isSelectAll) {
        detailedData = this.items;
      }
      let exportArray = [];

      // get detailed data
      for (let i = 0; i < detailedData.length; i++) {
        const {
          ie_year,
          ie_mon,
          fam_no,
          fam_cnt,
          job_cnt,
          job_typ_no,
          job_no,
        } = detailedData[i];

        const data = await this.queryDetailedData({
          Year: ie_year,
          Month: ie_mon,
          FamNo: fam_no,
        });

        exportArray = [
          ...exportArray,
          ...data.map(obj => {
            return {
              fam_cnt,
              job_cnt,
              job_typ_no,
              job_no,
              ...obj,
            };
          }),
        ];
      }

      // create txt data
      const csvData = [
        ...exportArray.map(obj => {
          const {
            ie_mon,
            ie_day,
            fam_no,
            fam_cnt,
            job_cnt,
            job_typ_no,
            job_no,
            code_no,
            code_amt,
            place,
            item_no,
          } = obj;

          return [
            `1${ie_mon}${ie_day}${fam_no}${code_no}0 0${fam_cnt}${job_cnt}${this.stringFixed(
              2,
              job_no
            )}${this.stringFixed(2, job_typ_no)}${this.stringFixed(
              7,
              code_amt
            )} ${this.stringFixed(6, item_no)} ${place}`,
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
      link.setAttribute(`download`, `incomestatement.txt`);
      document.body.appendChild(link);
      link.click();

      // clear
      this.clearSelected();
    },
    async exportCompleteCSV() {
      this.showProgressBar();
      this.pgbMax = this.items.length;

      let exportArray = [];

      // get detailed data
      for (let i = 0; i < this.items.length; i++) {
        const {
          ie_year,
          ie_mon,
          fam_no,
          fam_cnt,
          job_cnt,
          job_typ_no,
          job_no,
        } = this.items[i];

        const data = await this.queryDetailedData({
          Year: ie_year,
          Month: ie_mon,
          FamNo: fam_no,
        });

        exportArray = [
          ...exportArray,
          ...data.map(obj => {
            return {
              fam_cnt,
              job_cnt,
              job_typ_no,
              job_no,
              ...obj,
            };
          }),
        ];

        this.pgbValue = i;
      }

      // create txt data
      const csvData = [
        ...exportArray.map(obj => {
          const {
            ie_mon,
            ie_day,
            fam_no,
            fam_cnt,
            job_cnt,
            job_typ_no,
            job_no,
            code_no,
            code_amt,
            place,
            item_no,
            unit,
            qty,
            code_name,
          } = obj;

          return [
            `1`,
            `${ie_mon}${ie_day}`,
            fam_no,
            code_no,
            `0`,
            fam_cnt,
            job_cnt,
            job_typ_no,
            job_no,
            code_amt,
            item_no,
            place,
            unit,
            qty,
            code_name,
          ];
        }),
      ];
      const csvDataString = [
        [
          `類別`,
          `登打日期`,
          `記帳帳戶號`,
          `科目代碼`,
          `設算別`,
          `戶內人數`,
          `就業人數`,
          `本業：行業編號`,
          `本業：職業編號`,
          `金額`,
          `流水號`,
          `購買地`,
          `單位`,
          `數量`,
          `科目名稱`,
        ],
        ...csvData,
      ]
        .map(col => col.join(`,`))
        .join('\n');
      const encodedUri = URL.createObjectURL(
        new Blob([`\uFEFF${csvDataString}`], {
          type: `text/csv;charset=utf-8;`,
        })
      );

      this.hideProgressBar();

      // create link
      const link = document.createElement(`a`);
      link.setAttribute(`href`, encodedUri);
      link.setAttribute(`download`, `detailed_incomestatement.csv`);
      document.body.appendChild(link);
      link.click();
    },
    showDetails(famNo) {
      const { Year, Month } = this.queryObject;
      this.detailedQueryObj = {
        Year,
        Month,
        FamNo: famNo,
      };

      this.$refs.domModal.show();
    },
    async queryDetailedData(queryObject) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        return resObject.data.CoExpD || [];
      }

      return [];
    },
    stringFixed(number, tmepString) {
      if (tmepString.length === number || tmepString.length > number) {
        return tmepString;
      }

      const addNumber = number - tmepString.length;
      let addString = ``;
      for (let i = 0; i < addNumber; i++) {
        addString += `0`;
      }

      return `${addString}${tmepString}`;
    },
    async saveCoExp(insertItems) {
      const famNo = insertItems[0].fam_no;
      const ieYear = insertItems[0].ie_year;
      const ieMon = insertItems[0].ie_mon;
      const ieDay = insertItems[0].ie_day;
      const totalCost = insertItems.reduce((temp, obj) => {
        temp += parseInt(obj.code_amt, 10);
        return temp;
      }, 0);

      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.detatiledData,
        {
          Action: `WRITE`,
          InsertItems: JSON.stringify(insertItems),
          UpdateItems: JSON.stringify([]),
          FamNo: famNo,
          Year: ieYear,
          Month: ieMon,
          Day: ieDay,
          TotalCost: totalCost,
          Remark: ``,
        }
      );

      return resObject.status === this.mixinBackendErrorCode.success;
    },
    showProgressBar() {
      $(this.$refs.domProgressBG).css('display', 'block');
      $(this.$refs.domProgressBG).focus();
    },
    hideProgressBar() {
      $(this.$refs.domProgressBG).css('display', 'none');
      this.pgbValue = 0;
      this.pgbMax = 0;
    },
  },
  created() {
    this.fields = [
      { key: `selected`, label: `勾選` },
      { key: `ie_year`, label: `年` },
      { key: `ie_mon`, label: `月` },
      { key: `rec_name`, label: `登錄人員`, sortable: true },
      { key: `adi_name`, label: `審核人員`, sortable: true },
      { key: `fam_no`, label: `戶號`, sortable: true },
      { key: `state`, label: `資料狀態`, sortable: true },
      { key: `btnIncomeDetail`, label: `收支資料` },
    ];
  },
  async mounted() {
    // default load last month data
    const dtcurrent = new Date();
    dtcurrent.setMonth(dtcurrent.getMonth() - 1);
    const year = dtcurrent.getFullYear() - 1911;
    const month = dtcurrent.getMonth() + 1;

    this.queryObject.Year = year;
    this.queryObject.Month = month;

    await this.queryIncomeStateData(this.queryObject);
  },
  computed: {
    ...mapState(['codeAttrArray']),
    statusToString() {
      return statusCode => {
        return statusMapToString[statusCode];
      };
    },
    enabledConfirm() {
      if (this.selected.length === 0) {
        return false;
      }

      return (
        this.selected.every(obj => obj.state === `1`) &&
        ['A', 'B', 'C'].includes(this.mixinGetCookie('UserRole'))
      );
    },
    enabledReview() {
      if (this.selected.length === 0) {
        return false;
      }

      return (
        this.selected.every(obj => obj.state === `2`) &&
        ['A', 'C'].includes(this.mixinGetCookie('UserRole'))
      );
    },
    isSelectedItems() {
      if (this.selected.length === 0) {
        return false;
      }

      return this.selected.length > 0;
    },
    progress() {
      if (this.pgbMax === 0) {
        return '0%';
      }

      return `${Math.round((this.pgbValue / this.pgbMax) * 10000) / 100}%`;
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
  /* eslint-disable no-undef, no-param-reassign, camelcase, no-await-in-loop */
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
.blackBG {
  background-color: rgba(0, 0, 0, 0.7);
  width: 100%;
  height: 100%;
  position: fixed;
  left: 0;
  top: 0;
  text-align: center;
  display: none;
}
</style>
