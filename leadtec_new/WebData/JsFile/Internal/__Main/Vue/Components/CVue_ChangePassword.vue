<template>
  <div id="mainPage">
    <h5>變更密碼</h5>
    <div class="w-50 mx-auto my-3">
      <div style="border-bottom: 1px solid rgba(0, 0, 0, 0.1)">
        <label class="w-25">登入帳號</label>
        <p class="d-inline-block my-2 w-50 mx-4">{{ account }}</p>
      </div>
      <div style="border-bottom: 1px solid rgba(0, 0, 0, 0.1)">
        <label class="w-25" for="input-original">原密碼</label>
        <b-form-input
          id="input-original"
          v-model="originalPwd"
          :state="isValidOrigialPwd"
          @update="validPwd($event, false)"
          type="password"
          class="d-inline-block my-2 w-50 mx-4"
        ></b-form-input>
      </div>
      <div style="border-bottom: 1px solid rgba(0, 0, 0, 0.1)">
        <label class="w-25" for="input-newone">新密碼</label>
        <b-form-input
          id="input-newone"
          v-model="newPwd"
          :state="isValidNewPwd"
          @update="validPwd($event, true)"
          type="password"
          class="d-inline-block my-2 w-50 mx-4"
        ></b-form-input>
        <p style="font-size: 12px;color: red;">{{ newPwdErrorMsg }}</p>
      </div>
      <div style="border-bottom: 1px solid rgba(0, 0, 0, 0.1)">
        <label class="w-25" for="input-newone2">再次輸入新密碼</label>
        <b-form-input
          id="input-newone2"
          v-model="newPwd2"
          :state="newPwd2.length > 0 && newPwd === newPwd2"
          type="password"
          class="d-inline-block my-2 w-50 mx-4"
        ></b-form-input>
        <p></p>
      </div>
    </div>
    <p style="font-size: 12px;color: red;">
      密碼必須包含8個字符，需要包含英文、數字及符號(!@#$%^&*)，英文大小寫不一樣
    </p>
    <b-button
      variant="info"
      @click="changePassword"
      :disabled="!changeBtnEnable"
      class="my-2"
    >
      變更
    </b-button>
  </div>
</template>

<script>
const sha256 = require('js-sha256');

export default {
  /* eslint-disable no-undef, no-param-reassign */
  name: 'ChangePassword',
  components: {},
  props: {},
  data() {
    return {
      account: ``,
      originalPwd: ``,
      isValidOrigialPwd: false,
      newPwd: ``,
      isValidNewPwd: false,
      newPwd2: ``,
      newPwdErrorMsg: ``,
    };
  },
  methods: {
    validPwd(value, isNewPassword) {
      if (isNewPassword) {
        // length >= 8
        if (value.length >= 8 === false) {
          this.isValidNewPwd = false;
          this.newPwdErrorMsg = '密碼長度不足';
          return;
        }

        // contains english
        if (/[a-zA-Z]/g.test(value) === false) {
          this.isValidNewPwd = false;
          this.newPwdErrorMsg = '密碼沒有包含英文';
          return;
        }

        // contains number
        if (/[012345678]/g.test(value) === false) {
          this.isValidNewPwd = false;
          this.newPwdErrorMsg = '密碼沒有包含數字';
          return;
        }

        // contains char
        if (/[!@#$%^&*]/g.test(value) === false) {
          this.isValidNewPwd = false;
          this.newPwdErrorMsg = '密碼沒有包含特殊符號';
          return;
        }

        this.newPwdErrorMsg = '';
        this.isValidNewPwd = true;
      } else {
        this.isValidOrigialPwd = value.length > 0;
      }
    },
    async changePassword() {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.changePwd,
        {
          OPwd: sha256(this.originalPwd),
          NPwd: sha256(this.newPwd),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        this.$emit('confirm', false);
        alert(resObject.data);
        return;
      }

      this.originalPwd = ``;
      this.newPwd = ``;
      this.newPwd2 = ``;
      alert(`變更成功`);

      this.$emit('confirm', true);
    },
  },
  created() {},
  mounted() {
    this.account = this.mixinGetCookie('UserID');
  },
  computed: {
    changeBtnEnable() {
      return (
        this.isValidOrigialPwd &&
        this.isValidNewPwd &&
        this.newPwd === this.newPwd2
      );
    },
  },
  watch: {},
  /* eslint-disable no-undef, no-param-reassign */
};
</script>

<style scoped>
#mainPage {
  text-align: center;
  margin: auto;
}
</style>
