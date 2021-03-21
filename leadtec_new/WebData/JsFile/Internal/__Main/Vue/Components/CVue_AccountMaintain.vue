<template>
  <div id="mainPage">
    <h5>{{ title }}</h5>
    <p v-if="isEdit === false">預設密碼：P{{ tempUser.login_id }}</p>
    <b-container fluid>
      <b-row
        class="my-1"
        v-for="item in itemKeys"
        :key="item.key"
        v-if="item.isShow"
      >
        <b-col style="text-align: right;" col lg="2">
          <span v-if="item.isRequired" style="color: red">*</span>
          <label>{{ item.text }}</label>
        </b-col>
        <b-col style="text-align: left;" cols="8">
          <template v-if="item.key != `pwd`">
            <b-form-select
              v-if="item.type === `select`"
              v-model="tempUser[item.key]"
              :options="item.options"
              size="sm"
            ></b-form-select>
            <b-form-radio-group
              v-if="item.type === `radio`"
              v-model="tempUser[item.key]"
              :options="item.options"
              size="sm"
            ></b-form-radio-group>
            <b-form-input
              v-if="inputType.includes(item.type)"
              v-model="tempUser[item.key]"
              size="sm"
              value="null"
            ></b-form-input>
            <input
              v-if="item.type === `date`"
              type="date"
              v-model="tempUser[item.key]"
            />
          </template>
          <div v-else>
            <b-form-input
              v-if="isShowPwd"
              v-model="tempUser[item.key]"
              size="sm"
              value="null"
              :disabled="true"
            ></b-form-input>
            <b-form-input
              v-if="isResetPwd"
              v-model="newPwdString"
              size="sm"
              type="password"
              value="null"
              placeholder="請輸入新密碼"
              :state="isValidPwd"
            ></b-form-input>
            <p style="font-size: 12px;color: red;" v-if="isResetPwd">
              密碼必須包含8個字符，需要包含英文、數字及符號(!@#$%^&*)，英文大小寫不一樣
            </p>
            <b-button-group size="sm" class="my-2">
              <b-button @click="showPwd">{{ pwdMessageShowHide }}</b-button>
              <b-button @click="resetPwd">{{ resetPwdMessage }}</b-button>
            </b-button-group>
          </div>
        </b-col>
      </b-row>
    </b-container>

    <hr />

    <b-button variant="info" @click="save">儲存</b-button>
    <b-button variant="info" v-if="isEdit" @click="deleteUser">刪除</b-button>
  </div>
</template>

<script>
import { mapState } from 'vuex';
import { accountRole, accountStateToString } from '../DataModel/dataModel.js';

const sha256 = require('js-sha256');

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase, no-nested-ternary */
  name: 'AccountMaintain',
  components: {},
  props: {
    userObj: {
      type: Object,
      required: false,
    },
  },
  data() {
    return {
      isEdit: false,
      tempUser: {},
      allUserAccount: [],

      isShowPwd: false,
      isResetPwd: false,
      newPwdString: ``,
    };
  },
  methods: {
    async getAllAcount() {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.accountMgr,
        {
          Action: `READ`,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        this.allUserAccount = resObject.data.map(obj => obj.login_id);
      }
    },
    showPwd() {
      this.isShowPwd = !this.isShowPwd;
    },
    resetPwd() {
      this.isResetPwd = !this.isResetPwd;
    },
    save() {
      // check required items
      const isValid = this.itemKeys.every(obj => {
        if (obj.isRequired) {
          return this.tempUser[obj.key].length > 0;
        }

        return true;
      });

      if (isValid === false) {
        alert('請填寫有打 * 的項目');
        return;
      }

      // check duplicate
      if (
        this.isEdit === false &&
        this.allUserAccount.includes(this.tempUser.login_id)
      ) {
        alert('登入帳號不能重覆');
        return;
      }

      // process
      this.tempUser.pwd = this.isEdit
        ? this.isResetPwd
          ? sha256(this.newPwdString)
          : this.tempUser.pwd
        : sha256(`P${this.tempUser.login_id}`);
      this.tempUser.start_date = this.tempUser.start_date.replace(/-/g, '/');
      this.tempUser.end_date = this.tempUser.end_date.replace(/-/g, '/');
      this.tempUser.isResetPwd = this.isResetPwd;
      this.$emit(`save-user`, this.tempUser);
    },
    deleteUser() {
      this.$emit(`delete-user`, this.tempUser);
    },
  },
  created() {},
  async mounted() {
    if (Object.keys(this.userObj).length > 0) {
      this.tempUser = JSON.parse(JSON.stringify(this.userObj));

      this.tempUser.start_date = this.tempUser.start_date.replace(/\//g, '-');
      this.tempUser.end_date = this.tempUser.end_date.replace(/\//g, '-');

      this.isEdit = true;
    } else {
      await this.getAllAcount();
      const dtcurrent = new Date();

      this.tempUser = {
        email: ``,
        tel_no: ``,
        title: ``,
        remark: ``,
        dep_name: ``,
        role: ``,
        state: `1`,
        login_id: ``,
        pwd: ``,
        start_date: `${dtcurrent.getFullYear()}-${dtcurrent.getMonth() +
          1}-${dtcurrent.getDate()}`,
        end_date: ``,
      };
    }
  },
  computed: {
    ...mapState(['paramArray']),
    itemKeys() {
      return [
        {
          isShow: true,
          text: '登入帳號',
          key: 'login_id',
          isRequired: true,
          type: 'text',
        },
        {
          isShow: true,
          text: '姓名',
          key: 'user_name',
          isRequired: true,
          type: 'text',
        },
        {
          isShow: this.isEdit,
          text: '密碼',
          key: 'pwd',
          isRequired: false,
          type: 'text',
        },
        {
          isShow: true,
          text: '使用者角色',
          key: 'role',
          isRequired: true,
          type: 'radio',
          options: this.roleOpts,
        },
        {
          isShow: true,
          text: '使用者信箱',
          key: 'email',
          isRequired: false,
          type: 'text',
        },
        {
          isShow: true,
          text: '使用者電話',
          key: 'tel_no',
          isRequired: false,
          type: 'text',
        },
        {
          isShow: true,
          text: '職稱',
          key: 'title',
          isRequired: false,
          type: 'select',
          options: this.titleOpts,
        },
        {
          isShow: true,
          text: '機關單位',
          key: 'dep_name',
          isRequired: false,
          type: 'select',
          options: this.depOpts,
        },
        {
          isShow: true,
          text: '備註',
          key: 'remark',
          isRequired: false,
          type: 'text',
        },
        {
          isShow: true,
          text: '狀態',
          key: 'state',
          type: 'select',
          options: this.stateOpts,
          isRequired: true,
        },
        {
          isShow: true,
          text: '帳號開始日期',
          key: 'start_date',
          type: 'date',
          isRequired: true,
        },
        {
          isShow: true,
          text: '帳號結束日期',
          key: 'end_date',
          type: 'date',
          isRequired: false,
        },
      ];
    },
    roleOpts() {
      return Object.keys(accountRole).map(key => {
        return {
          text: accountRole[key],
          value: key,
        };
      });
    },
    stateOpts() {
      return Object.keys(accountStateToString).map(key => {
        return {
          text: accountStateToString[key],
          value: key,
        };
      });
    },
    titleOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `B`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_name,
          };
        });
    },
    depOpts() {
      return this.paramArray
        .filter(obj => obj.par_typ === `C`)
        .map(obj => {
          return {
            text: obj.par_name,
            value: obj.par_name,
          };
        });
    },
    inputType() {
      return [`text`];
    },
    title() {
      if (this.isEdit) {
        return `帳號編輯`;
      }
      return `帳號新增`;
    },
    pwdMessageShowHide() {
      if (this.isShowPwd) {
        return '隱藏密碼';
      }

      return '顯示密碼';
    },
    resetPwdMessage() {
      if (this.isResetPwd) {
        return '取消重設';
      }

      return '重設密碼';
    },
    isValidPwd() {
      const value = this.newPwdString;
      if (value.length < 8) {
        return false;
      }

      // contains english
      if (/[a-zA-Z]/g.test(value) === false) {
        return false;
      }

      // contains number
      if (/[012345678]/g.test(value) === false) {
        return false;
      }

      // contains char
      if (/[!@#$%^&*]/g.test(value) === false) {
        return false;
      }

      return true;
    },
  },
  watch: {},
};
/* eslint-disable no-undef, no-param-reassign, camelcase, no-nested-ternary */
</script>

<style scoped></style>
