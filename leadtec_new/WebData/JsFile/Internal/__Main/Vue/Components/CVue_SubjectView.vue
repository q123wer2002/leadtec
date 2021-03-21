<template>
  <div id="mainPage">
    <b-container fluid>
      <b-row
        class="my-1"
        v-for="item in itemKeys"
        :key="item.key"
        v-if="item.isShow"
      >
        <b-col style="text-align: right;" col lg="2">
          <label>{{ item.text }}</label>
        </b-col>
        <b-col style="text-align: left;" cols="8">
          <template>
            <b-form-select
              v-if="item.type === `select`"
              v-model="subjectData[item.key]"
              :options="placeOpts"
              size="sm"
              :disabled="item.isDisabled"
            ></b-form-select>
            <template v-if="item.type === `checkbox`">
              <b-form-checkbox
                v-if="item.key === `def_fg`"
                v-model="isDefaultName"
                size="sm"
              ></b-form-checkbox>
              <b-form-checkbox
                v-else
                v-model="isDisabled"
                size="sm"
                :disabled="item.isDisabled"
              ></b-form-checkbox>
            </template>
            <b-form-input
              v-if="inputType.includes(item.type)"
              v-model="subjectData[item.key]"
              size="sm"
              value="null"
              :disabled="item.isDisabled"
            ></b-form-input>
          </template>
        </b-col>
      </b-row>
    </b-container>

    <hr />

    <b-button variant="info" @click="save">儲存</b-button>
  </div>
</template>

<script>
import { mapState } from 'vuex';

export default {
  name: 'SubjectView',
  components: {},
  props: {
    subjectData: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      itemKeys: [
        {
          key: `code_no`,
          text: `科目代碼`,
          type: `text`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `code_name`,
          text: `科目名稱`,
          type: `text`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `upp_lim`,
          text: `金額上限`,
          type: `number`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `low_lim`,
          text: `金額下限`,
          type: `number`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `upp_sys`,
          text: `系統金額上限`,
          type: `number`,
          isShow: false,
          isDisabled: true,
        },
        {
          key: `low_sys`,
          text: `系統金額下限`,
          type: `number`,
          isShow: false,
          isDisabled: true,
        },
        {
          key: `place`,
          text: `購買地點`,
          type: `select`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `param1`,
          text: `科目設定1`,
          type: `text`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `param2`,
          text: `科目設定2`,
          type: `text`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `stop_fg`,
          text: `是否停用`,
          type: `checkbox`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `def_fg`,
          text: `預設名稱註記`,
          type: `checkbox`,
          isShow: true,
          isDisabled: false,
        },
        {
          key: `code_rem`,
          text: `備註`,
          type: `text`,
          isShow: true,
          isDisabled: false,
        },
      ],
      isEditView: false,
    };
  },
  methods: {
    setDefaultKeys() {
      this.subjectData.code_no = this.subjectData.code_no || ``;
      this.subjectData.code_name = this.subjectData.code_name || ``;
      this.subjectData.upp_lim = this.subjectData.upp_lim || ``;
      this.subjectData.low_lim = this.subjectData.low_lim || ``;
      this.subjectData.place = this.subjectData.place || ``;
      this.subjectData.param1 = this.subjectData.param1 || ``;
      this.subjectData.param2 = this.subjectData.param2 || ``;
      this.subjectData.stop_fg = this.subjectData.stop_fg || `N`;
      this.subjectData.code_rem = this.subjectData.code_rem || ``;
      this.subjectData.upp_sys = this.subjectData.upp_sys || ``;
      this.subjectData.low_sys = this.subjectData.low_sys || ``;
      this.subjectData.def_fg = this.subjectData.def_fg || ``;
    },
    initialColEnable() {
      if (this.isEditView === false) {
        return;
      }

      for (let i = 0; i < this.itemKeys.length; i++) {
        const { key } = this.itemKeys[i];
        if (key === `low_sys` || key === `upp_sys`) {
          this.itemKeys[i].isShow = true;
        }
      }
    },
    save() {
      this.$emit(`changed`, {
        subjectObj: this.subjectData,
        isDefaultName: this.isDefaultName,
      });
    },
    cancel() {
      this.$el.hide();
    },
  },
  created() {},
  mounted() {
    if (Object.keys(this.subjectData).length !== 0) {
      this.isEditView = true;
    }

    this.setDefaultKeys();
    this.initialColEnable();
  },
  computed: {
    ...mapState([`paramArray`]),
    isDisabled: {
      get() {
        return this.subjectData.stop_fg === `Y`;
      },
      set(value) {
        this.subjectData.stop_fg = value ? `Y` : `N`;
      },
    },
    isDefaultName: {
      get() {
        return this.subjectData.def_fg === `Y`;
      },
      set(value) {
        this.subjectData.def_fg = value ? `Y` : ``;
      },
    },
    placeOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `A`)
        .sort((obj1, obj2) => {
          const n1 = parseInt(obj1.par_no, 10);
          const n2 = parseInt(obj2.par_no, 10);

          if (n1 > n2) {
            return 1;
          }

          if (n1 === n2) {
            return 0;
          }

          return -1;
        })
        .map(obj => {
          return {
            value: obj.par_no,
            text: `${obj.par_no} ${obj.par_name}`,
          };
        });
    },
    inputType() {
      return [`text`, `number`];
    },
  },
  watch: {},
};
</script>

<style scoped></style>
