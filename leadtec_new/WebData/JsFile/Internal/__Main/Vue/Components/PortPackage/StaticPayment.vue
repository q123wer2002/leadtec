<template>
  <div id="mainPage">
    <h5>戶口組成 - 家庭固定支出繳費狀況</h5>

    <p class="my-2">
      {{ clonedData['date'] }}、{{ clonedData['fam_no'] }}、
      {{ clonedData['cou_name'] }}
    </p>

    <b-container fluid>
      <b-row class="my-1" v-for="item in fields" :key="item.key">
        <b-col style="text-align: right;">
          <label>{{ item.text }}</label>
        </b-col>
        <b-col style="text-align: left;">
          <b-form-input
            v-model="clonedData[item.key]"
            size="sm"
            value="null"
            class="w-25 d-inline"
          ></b-form-input>
        </b-col>
      </b-row>
    </b-container>
    <br />
    <b-button @click="saveItem" variant="info">儲存</b-button>
  </div>
</template>

<script>
export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'HouseInfo',
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
    fields() {
      return [
        {
          text: '水費(不含山泉水購買)',
          key: 'wat_fee',
          type: 'text',
        },
        {
          text: '電費',
          key: 'ele_fee',
          type: 'text',
        },
        {
          text: '市內電話費',
          key: 'int_tel_fee',
          type: 'text',
        },
        {
          text: '行動電話費',
          key: 'mob_fee',
          type: 'text',
        },
        {
          text: '網路月租費',
          key: 'net_fee',
          type: 'text',
        },
        {
          text: '天然瓦斯費(不含桶裝瓦斯)',
          key: 'gas_fee',
          type: 'text',
        },
        {
          text: '有線電視租用費',
          key: 'cab_fee',
          type: 'text',
        },
        {
          text: '多媒體隨選視訊月租費',
          key: 'mul_vedio_fee',
          type: 'text',
        },
        {
          text: '住宅(火險、竊盜險等)',
          key: 'hou_insu',
          type: 'text',
        },
        {
          text: '住宅管理保全費',
          key: 'hou_sec_fee',
          type: 'text',
        },
        {
          text: '停車位租用費',
          key: 'par_rent_fee',
          type: 'text',
        },
        {
          text: '保姆費',
          key: 'nanny',
          type: 'text',
        },
        {
          text: '佣人、司機費',
          key: 'driver',
          type: 'text',
        },
        {
          text: '實付房租',
          key: 'rent',
          type: 'text',
        },
        {
          text: '實付地租',
          key: 'rent_land',
          type: 'text',
        },
        {
          text: '學生就學住宿費',
          key: 'dorm_fee',
          type: 'text',
        },
        {
          text: '配(借)住設算',
          key: 'hou_loan_val',
          type: 'text',
        },
        {
          text: '自有自住設算',
          key: 'hou_own_val',
          type: 'text',
        },
        {
          text: '學生住校住宿費',
          key: 'dorm_sch_fee',
          type: 'text',
        },
        {
          text: '通勤費',
          key: 'commute',
          type: 'text',
        },
        {
          text: '補習費',
          key: 'tutoring',
          type: 'text',
        },
        {
          text: '房屋稅',
          key: 'hou_tax',
          type: 'text',
        },
        {
          text: '地價稅',
          key: 'land_tax',
          type: 'text',
        },
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
</style>
