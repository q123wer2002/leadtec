<template>
  <div id="mainPage">
    <h5>戶口組成 - 在學人數統計</h5>

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
          text: '托兒所幼稚園',
          key: 'nursery',
          type: 'text',
          unit: '人',
        },
        {
          text: '國小',
          key: 'elementary',
          type: 'text',
          unit: '人',
        },
        {
          text: '國中',
          key: 'junior',
          type: 'text',
          unit: '人',
        },
        {
          text: '高中',
          key: 'senior',
          type: 'text',
          unit: '人',
        },
        {
          text: '專科以上',
          key: 'college',
          type: 'text',
          unit: '人',
        },
        {
          text: '嬰幼兒由褓姆照顧',
          key: 'baby',
          type: 'text',
          unit: '人',
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
