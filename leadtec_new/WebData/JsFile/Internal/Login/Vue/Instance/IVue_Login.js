import Vue from 'vue';
import BootstrapVue from 'bootstrap-vue';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import vueStore from '../Vuex/Vuex_GlobalStore';
import '../Mixins/Vue_GlobalMixins.js';

import ValidCode from '../Components/CVue_ValidCode.vue';
import ChangePassword from '../../../Main/Vue/Components/CVue_ChangePassword.vue';

const sha256 = require('js-sha256');

Vue.use(BootstrapVue);

function IVueInitalLogin() {
  const _this = this;
  let _Instance = null;
  this.Intital = () => {
    _Instance = new Vue({
      /* eslint-disable no-undef no-param-reassign, no-restricted-globals */
      store: vueStore,
      el: '#vue_instance',
      components: {
        ValidCode,
        ChangePassword,
      },
      data: {
        userInput: {
          account: ``,
          password: ``,
          validCode: ``,
        },
        loginErrorMsg: ``,
        ip: ``,
        sysValidCode: ``,
      },
      watch: {},
      computed: {},
      methods: {
        async login() {
          // default
          this.loginErrorMsg = ``;

          // check is empty
          const { account, password, validCode } = this.userInput;
          if (account.length === 0 || password.length === 0) {
            this.loginErrorMsg = `帳號密碼不能為空`;
            return;
          }

          // check validcode
          if (validCode !== this.sysValidCode) {
            this.loginErrorMsg = '驗證碼錯誤';
            return;
          }

          const resObject = await this.mixinCallBackService(
            this.mixinBackendService.login,
            {
              Username: account,
              Password: sha256(password),
              IP: this.ip,
            }
          );

          const loginInfo = this.checkLoginResponse(resObject);
          if (loginInfo.isSuccess === false) {
            this.loginErrorMsg = loginInfo.message;
            return;
          }

          if (loginInfo.message.length > 0) {
            alert(loginInfo.message);
          }

          // re-direct to home page
          this.mixinToHomePage();
        },
        async checkAccountStatus() {
          const { isSuccess } = await this.mixinAccountStatus();

          // direct to login page
          if (isSuccess) {
            this.mixinToHomePage();
          }
        },
        checkLoginResponse(resObject) {
          const loginInfo = {
            isSuccess: false,
            message: ``,
          };

          switch (resObject.status) {
            case 0:
              loginInfo.isSuccess = true;
              break;
            case 1: // change password
              this.$refs.changePasswordDiv.show();
              break;
            case 2:
              loginInfo.isSuccess = true;
              loginInfo.message = `帳號即將於 ${
                resObject.data
              } 過期，記得更換密碼`;
              break;
            case -1:
            case -2:
              loginInfo.message = '帳號或密碼錯誤';
              break;
            case -4:
              loginInfo.message = '帳號已被鎖住，請通知系統管理人員';
              break;
            case -7:
              loginInfo.message = '帳號已過期，請通知系統維護人員';
              break;
            default:
              loginInfo.message = '';
              break;
          }

          return loginInfo;
        },
        changePwdAction(result) {
          if (result === false) {
            return;
          }

          location.reload(true);
        },
      },
      created() {},
      async mounted() {
        // check account status
        await this.checkAccountStatus();

        const ipInfo = await this.mixinGetIpInfo();
        this.ip = ipInfo == null ? `` : ipInfo.ip;
      },
      /* eslint-disable no-undef no-param-reassign, no-restricted-globals */
    });
  };
  this._getColsure = () => {
    return { _this, _Instance };
  };
}

(function initIVue(window) {
  /* eslint-disable no-param-reassign */
  window.IncomeStatement = window.IncomeStatement || {};
  window.IncomeStatement.js_Vue_Instance =
    window.IncomeStatement.js_Vue_Instance || {};
  window.IncomeStatement.js_Vue_Instance.IVue_Login = new IVueInitalLogin();
  /* eslint-disable no-param-reassign */
})(window);
