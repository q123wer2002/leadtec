<template>
  <div id="mainPage">
    <h5>主卡</h5>

    <b-container fluid>
      <b-row class="my-1" v-for="item in fields" :key="item.key">
        <b-col style="text-align: right;" col lg="3">
          <label>{{ item.text }}</label>
        </b-col>
        <b-col style="text-align: left;" cols="8">
          <template>
            <span v-if="item.type === `span`">{{ clonedData[item.key] }}</span>
            <b-form-select
              v-if="item.type === `select`"
              v-model="clonedData[item.key]"
              :options="item.options"
              size="sm"
            ></b-form-select>
            <b-form-input
              v-if="inputType.includes(item.type)"
              v-model="clonedData[item.key]"
              size="sm"
              value="null"
            ></b-form-input>
          </template>
        </b-col>
      </b-row>
    </b-container>
    <br />
    <b-button @click="saveItem" variant="info">儲存</b-button>
  </div>
</template>

<script>
import { mapState } from 'vuex';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'MainCard',
  components: {},
  props: {
    coFamData: {
      type: Object,
      reqiured: true,
    },
  },
  data() {
    return {
      clonedData: {},
    };
  },
  methods: {
    saveItem() {
      const saveData = JSON.parse(JSON.stringify(this.clonedData));
      delete saveData.date;

      this.$emit('updateFamItem', saveData);
    },
  },
  created() {},
  mounted() {
    this.clonedData = JSON.parse(JSON.stringify(this.coFamData));

    // add date
    const { ie_year, ie_mon } = this.clonedData;
    this.clonedData.date = `${ie_year}年${ie_mon}月`;
  },
  computed: {
    ...mapState(['paramArray']),
    fields() {
      return [
        {
          text: '年月',
          type: 'span',
          key: 'date',
        },
        {
          text: '縣市村里名稱',
          type: 'span',
          key: 'cou_name',
        },
        {
          text: '戶號',
          type: 'span',
          key: 'fam_no',
        },
        {
          text: '戶長名稱',
          type: 'text',
          key: 'fam_head',
        },
        {
          text: '戶內人數',
          type: 'text',
          key: 'fam_cnt',
        },
        {
          text: '就業人數',
          type: 'text',
          key: 'job_cnt',
        },
        {
          text: '是否換戶',
          type: 'select',
          key: 'fam_cha',
          options: [
            {
              text: '無換戶',
              value: '無換戶',
            },
            {
              text: '換戶',
              value: '換戶',
            },
          ],
        },
        {
          text: '收入級距',
          type: 'select',
          key: 'ie_lev',
          options: this.paramArray
            .filter(obj => obj.par_typ === 'D')
            .map(obj => {
              return {
                text: obj.par_name,
                value: parseInt(obj.par_no, 10).toString(),
              };
            }),
        },
        {
          text: '電話',
          type: 'text',
          key: 'phone',
        },
        {
          text: '樣本名冊流水號',
          type: 'text',
          key: 'sam_no',
        },
        {
          text: '地址',
          type: 'text',
          key: 'fam_addr',
        },
        {
          text: '備註',
          type: 'text',
          key: 'fam_remark',
        },
        {
          text: '訪問員',
          type: 'span',
          key: 'int_name',
        },
        {
          text: '整理員',
          type: 'span',
          key: 'sor_name',
        },
        {
          text: '審核員',
          type: 'span',
          key: 'adi_name',
        },
        {
          text: '登錄員',
          type: 'span',
          key: 'rec_name',
        },
      ];
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
