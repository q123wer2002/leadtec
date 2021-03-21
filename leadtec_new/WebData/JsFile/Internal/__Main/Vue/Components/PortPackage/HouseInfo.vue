<template>
  <div id="mainPage">
    <h5>戶口組成 - 住宅概況</h5>

    <p class="my-2">
      {{ clonedData['date'] }}、{{ clonedData['fam_no'] }}、
      {{ clonedData['cou_name'] }}
    </p>

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
            <b-form-checkbox-group
              v-if="item.type === `checkbox`"
              v-model="clonedData[item.key]"
              :options="item.options"
            ></b-form-checkbox-group>
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
      // set par data
      const parValue = this.clonedData.par;
      if (parValue !== undefined) {
        switch (parValue) {
          case 0:
            this.clonedData.par_none = `1`;
            this.clonedData.par_self = ``;
            this.clonedData.par_rent = ``;
            break;
          case 1:
            this.clonedData.par_none = ``;
            this.clonedData.par_self = `1`;
            this.clonedData.par_rent = ``;
            break;
          case 2:
            this.clonedData.par_none = ``;
            this.clonedData.par_self = ``;
            this.clonedData.par_rent = `1`;
            break;
          default:
            break;
        }
      }

      const saveData = JSON.parse(JSON.stringify(this.clonedData));
      delete saveData.date;
      delete saveData.par;

      this.$emit('updateFamItem', saveData);
    },
  },
  created() {},
  mounted() {
    this.clonedData = JSON.parse(JSON.stringify(this.coFamData));

    // add date
    const { ie_year, ie_mon, par_none, par_self, par_rent } = this.clonedData;
    this.clonedData.date = `${ie_year}年${ie_mon}月`;

    // add par
    let parValue;
    if (par_none) {
      parValue = 0;
    } else if (par_self) {
      parValue = 1;
    } else if (par_rent) {
      parValue = 2;
    }
    this.clonedData.par = parValue;
  },
  computed: {
    fields() {
      return [
        {
          text: '住宅所屬',
          type: 'select',
          key: 'hou_own',
          options: [
            {
              text: '租押',
              value: '租押',
            },
            {
              text: '自有',
              value: '自有',
            },
          ],
        },
        {
          text: '現住自宅房屋貸款',
          type: 'select',
          key: 'hou_loan',
          options: [
            {
              text: '有',
              value: '有',
            },
            {
              text: '無',
              value: '無',
            },
          ],
        },
        {
          text: '用途',
          type: 'select',
          key: 'hou_use',
          options: [
            {
              text: '專用',
              value: '專用',
            },
            {
              text: '併用',
              value: '併用',
            },
          ],
        },
        {
          text: '建築式樣',
          type: 'select',
          key: 'hou_typ',
          options: [
            {
              text: '平房',
              value: '平房',
            },
            {
              text: '2~3層樓',
              value: '2~3層樓',
            },
            {
              text: '4~5層樓',
              value: '4~5層樓',
            },
            {
              text: '6層樓以上',
              value: '6層樓以上',
            },
          ],
        },
        {
          text: '自來水設備',
          type: 'select',
          key: 'tap_wat',
          options: [
            {
              text: '有',
              value: '有',
            },
            {
              text: '無',
              value: '無',
            },
          ],
        },
        {
          text: '有車者之停車位',
          type: 'checkbox',
          key: 'par',
          options: [
            {
              text: `無`,
              value: 0,
            },
            {
              text: `自有`,
              value: 1,
            },
            {
              text: `租借`,
              value: 2,
            },
          ],
        },
        {
          text: '住宅面積-佔地',
          type: 'text',
          key: 'hou_land',
        },
        {
          text: '住宅面積-建坪',
          type: 'text',
          key: 'hou_ping',
        },
        {
          text: '地址',
          type: 'text',
          key: 'fam_addr',
        },
        {
          text: '住宅管理費',
          type: 'text',
          key: 'hou_man_fee',
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
