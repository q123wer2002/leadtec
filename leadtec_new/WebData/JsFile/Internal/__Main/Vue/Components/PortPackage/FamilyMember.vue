<template>
  <div id="mainPage">
    <h5>戶口組成資料</h5>

    <p class="my-2">
      {{ titleString }}
    </p>

    <b-table
      :items="items"
      :fields="fields"
      hover
      small
      head-variant="dark"
      @row-clicked="onRowClicked"
    >
      <template slot="[action]" slot-scope="data">
        <a href="javascript:;" @click="deleteMember(data.index)">
          <img :src="$options.imgSrc.trash" width="32px" />
        </a>
      </template>
      <b-form-input
        slot="[mem_no]"
        slot-scope="data"
        v-model="data.item.mem_no"
        style="width:60px"
      ></b-form-input>
      <b-form-input
        slot="[title]"
        slot-scope="data"
        v-model="data.item.title"
        style="width:80px"
      ></b-form-input>
      <b-form-input
        slot="[fam_head_rel]"
        slot-scope="data"
        v-model="data.item.fam_head_rel"
        style="width:50px"
      ></b-form-input>
      <b-form-input
        slot="[mem_name]"
        slot-scope="data"
        v-model="data.item.mem_name"
        style="width:120px"
      ></b-form-input>
      <b-form-select
        slot="[gender]"
        slot-scope="data"
        v-model="data.item.gender"
        :options="genderOpts"
        style="width:70px"
      ></b-form-select>
      <b-form-input
        slot="[bir_year]"
        slot-scope="data"
        v-model="data.item.bir_year"
        @update="calculateOld($event, data.index)"
        style="width:70px"
      ></b-form-input>
      <b-form-select
        slot="[bir_mon]"
        slot-scope="data"
        v-model="data.item.bir_mon"
        :options="monthOpts"
        style="width:100px"
      ></b-form-select>
      <b-form-select
        slot="[edu_no]"
        slot-scope="data"
        v-model="data.item.edu_no"
        :options="eduNoOpts"
        style="width:80px"
      ></b-form-select>
      <b-form-select
        slot="[job_typ]"
        slot-scope="data"
        v-model="data.item.job_typ"
        :options="jobTypeOpts"
        style="width:80px"
      ></b-form-select>
      <b-form-input
        slot="[mem_remark]"
        slot-scope="data"
        v-model="data.item.mem_remark"
        style="width:200px"
      ></b-form-input>
      <template slot="bottom-row" slot-scope="data">
        <b-th :colspan="fields.length">
          <b-button size="sm" @click="addNewMember">新增戶口組成</b-button>
        </b-th>
      </template>
    </b-table>
    <hr />
    <span v-if="selectedFamUniData === ``">請先點選成員</span>
    <b-container v-else>
      <div
        class="d-inline-block w-50 my-2"
        style="text-align: left;"
        v-for="item in colFields"
        :key="item.key"
      >
        <label style="width: 200px;text-align: right;">{{ item.text }}</label>
        <div class="w-50 d-inline-block">
          <span v-if="item.type === `span`">{{
            selectedFamUniData[item.key]
          }}</span>
          <b-form-select
            v-if="item.type === `select`"
            v-model="selectedFamUniData[item.key]"
            :options="item.options"
            size="sm"
          ></b-form-select>
          <b-form-checkbox-group
            v-if="item.type === `checkbox`"
            v-model="selectedFamUniData[item.key]"
            :options="item.options"
          ></b-form-checkbox-group>
          <b-form-input
            v-if="inputType.includes(item.type)"
            v-model="selectedFamUniData[item.key]"
            size="sm"
            value="null"
          ></b-form-input>
          <b-form-select
            v-if="item.type === `eduType`"
            v-model="selectedFamUniData[item.key]"
            :options="educationOpts"
          ></b-form-select>
        </div>
      </div>
    </b-container>
    <br />
    <b-button @click="saveItem" variant="info">儲存</b-button>
  </div>
</template>

<script>
import { mapState } from 'vuex';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  imgSrc: {
    trash: `/${webpackDashboardName}/WebData/Picture/icon/material-io/baseline_delete_forever_black_48dp.png`,
  },

  name: 'HouseInfo',
  components: {},
  props: {
    coFamData: {
      type: Array,
      reqiured: true,
    },
  },
  data() {
    return {
      items: {},
      selectedFamUniData: ``,
    };
  },
  methods: {
    saveItem() {
      this.$emit(`updateFamItem`, this.items);
    },
    onRowClicked(item) {
      this.selectedFamUniData = item;
    },
    addNewMember() {
      const copiedItem = JSON.parse(JSON.stringify(this.items[0]));
      const cannotModify = [`ie_year`, `ie_mon`, `fam_no`];
      Object.keys(copiedItem).forEach(key => {
        if (cannotModify.includes(key) === false) {
          copiedItem[key] = ``;
        }
      });

      this.items.push(copiedItem);
    },
    deleteMember(index) {
      if (this.items.length === 1) {
        alert('剩下最後一個人，不能刪除');
        return;
      }

      this.items.splice(index, 1);
    },
    calculateOld(year, index) {
      const today = new Date();
      const newAge = today.getFullYear() - 1911 - parseInt(year, 10);
      if (newAge) {
        this.items[index].age = newAge;
      }
    },
  },
  created() {
    this.items = JSON.parse(JSON.stringify(this.coFamData));
  },
  mounted() {},
  computed: {
    ...mapState(['paramArray']),
    fields() {
      return [
        {
          label: '操作',
          key: 'action',
        },
        {
          label: '代號',
          key: 'mem_no',
        },
        {
          label: '稱謂',
          key: 'title',
        },
        {
          label: '與戶長關係代號',
          key: 'fam_head_rel',
        },
        {
          label: '姓名',
          key: 'mem_name',
        },
        {
          label: '性別',
          key: 'gender',
        },
        {
          label: '出生年',
          key: 'bir_year',
        },
        {
          label: '出生月',
          key: 'bir_mon',
        },
        {
          label: '年齡',
          key: 'age',
        },
        {
          label: '最高教育程度',
          key: 'edu_no',
        },
        {
          label: '就業別',
          key: 'job_typ',
        },
        {
          label: '備註',
          key: 'mem_remark',
        },
      ];
    },
    colFields() {
      return [
        {
          text: '學校公私立',
          key: 'sch_typ',
          type: 'select',
          options: [
            {
              text: '公立',
              value: '公立',
            },
            {
              text: '私立',
              value: '私立',
            },
          ],
        },
        {
          text: '教育程度',
          key: 'education',
          type: 'eduType',
          // options: this.eduNoOpts,
        },
        {
          text: '學校',
          key: 'sch_name',
          type: 'text',
        },
        {
          text: '年級',
          key: 'grade',
          type: 'text',
        },
        {
          text: '擔任職務',
          key: 'job_title',
          type: 'text',
        },
        {
          text: '機關全銜',
          key: 'job_com',
          type: 'text',
        },
        {
          text: '行業編號',
          key: 'job_typ_no',
          type: 'select',
          options: this.jobTypeNoOpts,
        },
        {
          text: '職業編號',
          key: 'job_no',
          type: 'select',
          options: this.jobNoOpts,
        },
        {
          text: '所得提供家庭(%)',
          key: 'inc_fam_prc',
          type: 'text',
        },
        {
          text: '費用家庭供給(%)',
          key: 'fee_fam_prc',
          type: 'text',
        },
        {
          text: '社會保險繳納金額(元)',
          key: 'insu_soci_amt',
          type: 'text',
        },
        {
          text: '健保繳納金額(元)',
          key: 'insu_heal_amt',
          type: 'text',
        },
        {
          text: '公保',
          key: 'insu_pub',
          type: 'text',
        },
        {
          text: '一般勞保',
          key: 'insu_lab',
          type: 'text',
        },
        {
          text: '農保',
          key: 'insu_farm',
          type: 'text',
        },
        {
          text: '漁會甲類',
          key: 'insu_fish',
          type: 'text',
        },
        {
          text: '軍保',
          key: 'insu_mil',
          type: 'text',
        },
        {
          text: '全民健保',
          key: 'insu_heal',
          type: 'text',
        },
        {
          text: '國民年金',
          key: 'annuity',
          type: 'text',
        },
        {
          text: '健康醫療險',
          key: 'insu_medi',
          type: 'text',
        },
        {
          text: '意外傷害險',
          key: 'insu_acci',
          type: 'text',
        },
        {
          text: '定期終身壽險',
          key: 'insu_life',
          type: 'text',
        },
        {
          text: '汽車保險',
          key: 'insu_car',
          type: 'text',
        },
        {
          text: '機車保險',
          key: 'insu_moto',
          type: 'text',
        },
      ];
    },
    titleString() {
      if (this.items.length <= 0) {
        return '';
      }

      const { ie_year, ie_mon, fam_no } = this.items[0];
      return `年月：${ie_year}年${ie_mon}月、戶號：${fam_no}`;
    },
    genderOpts() {
      return [
        {
          text: '男',
          value: '男',
        },
        {
          text: '女',
          value: '女',
        },
      ];
    },
    monthOpts() {
      return [
        `01`,
        `02`,
        `03`,
        `04`,
        `05`,
        `06`,
        `07`,
        `08`,
        `09`,
        `10`,
        `11`,
        `12`,
      ].map(month => {
        return {
          text: `${month}月`,
          value: parseInt(month, 10),
        };
      });
    },
    eduNoOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `E`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_no,
          };
        });
    },
    educationOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `E`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_name,
          };
        });
    },
    jobTypeOpts() {
      return [
        {
          text: `就業`,
          value: 1,
        },
        {
          text: `未就業`,
          value: 2,
        },
      ];
    },
    jobNoOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `G`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_no,
          };
        });
    },
    jobTypeNoOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `F`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_no,
          };
        });
    },
    inputType() {
      return [`text`, `number`];
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
</style>
