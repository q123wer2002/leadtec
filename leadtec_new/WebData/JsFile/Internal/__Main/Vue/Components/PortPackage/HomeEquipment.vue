<template>
  <div id="mainPage">
    <h5>戶口組成 - 家庭設備概況</h5>

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
          <span>{{ item.unit }}</span>
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
          text: '彩色電視機',
          type: 'text',
          key: 'col_tv',
          unit: '台',
        },
        {
          text: '液晶、電漿電視機',
          type: 'text',
          key: 'led_tv',
          unit: '台',
        },
        {
          text: '數位影音光碟機',
          type: 'text',
          key: 'dvd_palyer',
          unit: '台',
        },
        {
          text: '攝影機',
          type: 'text',
          key: 'camera',
          unit: '架',
        },
        {
          text: '電視遊樂器',
          type: 'text',
          key: 'tv_game',
          unit: '台',
        },
        {
          text: '多媒體隨選視訊',
          type: 'text',
          key: 'mul_vedio',
          unit: '',
        },
        {
          text: '有線電視頻道設備',
          type: 'text',
          key: 'cable_tv',
          unit: '',
        },
        {
          text: '家用電腦',
          type: 'text',
          key: 'computer',
          unit: '組',
        },
        {
          text: '數位相機',
          type: 'text',
          key: 'dig_camera',
          unit: '台',
        },
        {
          text: '汽車',
          type: 'text',
          key: 'car',
          unit: '輛',
        },
        {
          text: '機車',
          type: 'text',
          key: 'moto',
          unit: '輛',
        },
        {
          text: '洗衣機',
          type: 'text',
          key: 'wash_mach',
          unit: '台',
        },
        {
          text: '烘衣機',
          type: 'text',
          key: 'dryer',
          unit: '台',
        },
        {
          text: '空氣清靜機',
          type: 'text',
          key: 'air_puri',
          unit: '台',
        },
        {
          text: '開飲機',
          type: 'text',
          key: 'dri_mach',
          unit: '台',
        },
        {
          text: '微波爐',
          type: 'text',
          key: 'mic_oven',
          unit: '台',
        },
        {
          text: '報紙',
          type: 'text',
          key: 'newspaper',
          unit: '份',
        },
        {
          text: '音響',
          type: 'text',
          key: 'audio',
          unit: '組',
        },
        {
          text: '鋼琴(含電子琴)',
          type: 'text',
          key: 'piano',
          unit: '組',
        },
        {
          text: '除濕機',
          type: 'text',
          key: 'dehumidifier',
          unit: '台',
        },
        {
          text: '電話機',
          type: 'text',
          key: 'telphone',
          unit: '架',
        },
        {
          text: '行動電話',
          type: 'text',
          key: 'mobile',
          unit: '架',
        },
        {
          text: '熱水器',
          type: 'text',
          key: 'wat_heater',
          unit: '台',
        },
        {
          text: '電磁爐',
          type: 'text',
          key: 'ind_cooker',
          unit: '台',
        },
        {
          text: '冷暖氣機',
          type: 'text',
          key: 'air_cond',
          unit: '台',
        },
        {
          text: '濾水器',
          type: 'text',
          key: 'wat_filter',
          unit: '台',
        },
        {
          text: '吸塵器',
          type: 'text',
          key: 'vac_cleaner',
          unit: '部',
        },
        {
          text: '期刊雜誌',
          type: 'text',
          key: 'magazine',
          unit: '份',
        },
        {
          text: '錄放影機',
          type: 'text',
          key: 'vedio',
          unit: '台',
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
