<template>
  <div id="mainPage">
    <h5>帳號維護</h5>
    <selector
      :filterModel="selectorModel"
      :isDetailed="true"
      @additem="addUser"
      @search="searchEvent"
    ></selector>

    <div id="tableData">
      <div class="w-25 d-inline-block justify-content-center">
        <label class="d-inline-block">每頁顯示筆數</label>
        <b-form-input
          v-model="perPage"
          type="number"
          size="sm"
          class="d-inline-block"
        ></b-form-input>
      </div>
      <b-table
        ref="domDatatable"
        :items="items"
        :fields="fields"
        :per-page="perPage"
        :current-page="currentPage"
        hover
        small
        head-variant="dark"
        class="my-2"
      >
        <span slot="[role]" slot-scope="data">
          {{ roleToString(data.item.role) }}
        </span>
        <span slot="[state]" slot-scope="data">
          {{ stateToString(data.item.state) }}
        </span>
        <template slot="[action]" slot-scope="data">
          <a href="javascript:;" @click="editUser(data.item)">編輯</a>
        </template>
      </b-table>
      <b-pagination
        v-model="currentPage"
        :total-rows="items.length"
        :per-page="perPage"
        size="sm"
        class="justify-content-center"
      ></b-pagination>
    </div>

    <b-modal ref="domModal" size="xl" hide-footer>
      <AccountMaintain
        :user-obj="selectedUser"
        @save-user="onSaveUser"
        @delete-user="onDeleteUser"
      ></AccountMaintain>
    </b-modal>
  </div>
</template>

<script>
import { accountStateToString, accountRole } from '../DataModel/dataModel.js';
import { accountModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import AccountMaintain from './CVue_AccountMaintain.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  name: 'AccountManagement',
  components: {
    Selector,
    AccountMaintain,
  },
  props: {},
  data() {
    return {
      // for selector
      selectorModel: accountModel,
      queryObject: {},
      selectedUser: {},

      // for table
      items: [],
      currentPage: 1,
      perPage: 20,
    };
  },
  methods: {
    // filter
    async searchEvent(filterObject) {
      this.queryObject = {};
      const { account, name, status } = filterObject;

      if (account.id.length > 0) {
        this.queryObject.Account = account.id;
      }

      if (name.value.length > 0) {
        this.queryObject.Name = name.value;
      }

      this.queryObject.Status = status.code;
      await this.queryAccount(this.queryObject);
    },

    // add user
    addUser() {
      this.selectedUser = {};
      this.$refs.domModal.show();
    },
    editUser(userObject) {
      this.selectedUser = userObject;
      this.$refs.domModal.show();
    },
    async onSaveUser(userObject) {
      // set end date
      if (userObject.end_date.length === 0) {
        const date = new Date(userObject.start_date);
        date.setYear(date.getFullYear() + 2);
        userObject.end_date = `${date.getFullYear()}/${date.getMonth() +
          1}/${date.getDate()}`;
      }

      if (
        this.selectedUser.login_id &&
        this.selectedUser.login_id !== userObject.login_id
      ) {
        // it needs to delete user, and insert new one
        await this.deleteUser(this.selectedUser);
        await this.updateInsertUser(userObject);
        return;
      }

      await this.updateInsertUser(userObject);
      this.$refs.domModal.hide();
    },
    async onDeleteUser(userObject) {
      await this.deleteUser(userObject);
      this.$refs.domModal.hide();
    },

    // ui show
    roleToString(role) {
      return accountRole[role];
    },
    stateToString(state) {
      return accountStateToString[state];
    },

    // backend api
    async queryAccount(queryObject) {
      this.items = [];
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.accountMgr,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      this.items = resObject.data.map(obj => {
        return {
          ...obj,
          start_date: obj.start_date.split(` `)[0],
          end_date: obj.end_date.split(` `)[0],
        };
      });
    },
    async updateInsertUser(userObj) {
      console.log(userObj);
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.accountMgr,
        {
          Action: `WRITE`,
          UserObject: JSON.stringify(userObj),
          ResetPwd: userObj.isResetPwd || false,
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        alert(`儲存失敗，${resObject.data}`);
        return;
      }

      alert(`儲存成功`);
      const userIdx = this.items.findIndex(
        obj => obj.login_id === userObj.login_id
      );
      if (userIdx === -1) {
        this.items.push(userObj);
        return;
      }

      this.$set(this.items, userIdx, userObj);
    },
    async deleteUser(userObj) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.accountMgr,
        {
          Action: `DELETE`,
          UserObject: JSON.stringify(userObj),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      const userIdx = this.items.findIndex(
        obj => obj.login_id === userObj.login_id
      );
      this.$delete(this.items, userIdx);
    },
  },
  created() {},
  mounted() {},
  computed: {
    fields() {
      return [
        { key: `login_id`, label: `登入帳號` },
        { key: `user_name`, label: `姓名` },
        { key: `role`, label: `使用者角色` },
        { key: `start_date`, label: `帳號開始日期` },
        { key: `end_date`, label: `帳號結束日期` },
        { key: `state`, label: `狀態` },
        { key: `action`, label: `` },
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
  margin: auto;
}
#tableData {
  width: 85%;
  margin: auto;
}
</style>
