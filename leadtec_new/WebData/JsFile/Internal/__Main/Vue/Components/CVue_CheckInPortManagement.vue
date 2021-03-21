<template>
  <div id="mainPage">
    <h5>登錄戶號管理</h5>
    <selector
      :filterModel="selectorModel"
      :isDetailed="true"
      @additem="addItem"
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
        <template slot="[action]" slot-scope="data">
          <a href="javascript:;" @click="openDeleteDialog(data.item)">
            <img width="24px" :src="$options.imgSrc.trash" />
          </a>
          <a href="javascript:;" @click="editItem(data.item)">
            <img width="24px" :src="$options.imgSrc.edit" />
          </a>
        </template>
        <span slot="[rec_status]" slot-scope="data">
          {{ accountStateToString(data.item.rec_status) }}
        </span>
        <span slot="[adi_status]" slot-scope="data">
          {{ accountStateToString(data.item.adi_status) }}
        </span>
        <span slot="[state]" slot-scope="data">
          {{ stateToString(data.item.state) }}
        </span>
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
      <CheckInMaintain
        :famObj="selectedFamObj"
        @save-fam="onSaveFam"
      ></CheckInMaintain>
    </b-modal>
  </div>
</template>

<script>
import { portState, accountStateToString } from '../DataModel/dataModel.js';
import { checkinPortModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import CheckInMaintain from './CVue_CheckInPortMaintain.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase */
  imgSrc: {
    trash: `/${webpackDashboardName}/WebData/Picture/icon/material-io/baseline_delete_forever_black_48dp.png`,
    edit: `/${webpackDashboardName}/WebData/Picture/icon/material-io/baseline_edit_black_48dp.png`,
  },

  name: 'CheckInManagement',
  components: {
    Selector,
    CheckInMaintain,
  },
  props: {},
  data() {
    return {
      // for selector
      selectorModel: checkinPortModel,
      queryObject: {},
      selectedFamObj: {},

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
      const { loginman, port, reviewman } = filterObject;

      if (loginman.id.length > 0) {
        this.queryObject.ChechInNo = loginman.id;
      }

      if (reviewman.id.length > 0) {
        this.queryObject.ReviewNo = reviewman.id;
      }

      if (
        port.end.length !== 0 &&
        port.start.length !== 0 &&
        port.end >= port.start
      ) {
        this.queryObject.FamNoStart = port.start;
        this.queryObject.FamNoEnd = port.end;
      }

      await this.queryAccount(this.queryObject);
    },
    addItem() {
      this.selectedFamObj = {};
      this.$refs.domModal.show();
    },
    editItem(item) {
      this.selectedFamObj = item;
      this.$refs.domModal.show();
    },
    async onSaveFam(famList) {
      const originalFamNos = this.items.map(obj => obj.fam_no);
      const updateItems = famList.filter(obj =>
        originalFamNos.includes(obj.fam_no)
      );
      const insertItems = famList.filter(
        obj => originalFamNos.includes(obj.fam_no) === false
      );

      if (updateItems && updateItems.length > 0) {
        await this.updateFamObj(updateItems);
      }

      if (insertItems && insertItems.length > 0) {
        await this.insertFamObj(insertItems);
      }

      this.$refs.domModal.hide();
    },

    // ui show
    stateToString(state) {
      return portState[state];
    },
    accountStateToString(state) {
      return accountStateToString[state];
    },
    updateFam() {
      this.$refs.domModal.hide();
    },
    insertFam() {
      this.$refs.domModal.hide();
    },
    openDeleteDialog(item) {
      this.$bvModal
        .msgBoxConfirm(`是否要刪除 ${item.fam_no}`, {
          title: '請確認',
          size: 'sm',
          buttonSize: 'sm',
          okVariant: 'danger',
          okTitle: '是',
          cancelTitle: '否',
          footerClass: 'p-2',
          hideHeaderClose: false,
          centered: true,
        })
        .then(async value => {
          if (value) {
            await this.deleteFamObj(item);
          }
        })
        .catch(err => {
          // An error occurred
          console.error(err);
        });
    },

    // backend api
    async queryAccount(queryObject) {
      this.items = [];
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.famInfo,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      this.items = resObject.data;
    },
    async deleteFamObj(item) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.famInfo,
        {
          Action: `DELETE`,
          FamObject: JSON.stringify(item),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      const { fam_no, rec_user } = item;
      const famIdx = this.items.findIndex(
        obj => obj.fam_no === fam_no && obj.rec_user === rec_user
      );
      this.$delete(this.items, famIdx);
    },
    async updateFamObj(updateItems) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.famInfo,
        {
          Action: `UPDATE`,
          FamArray: JSON.stringify(updateItems),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      for (let i = 0; i < updateItems.length; i++) {
        const itemObj = updateItems[i];
        const itemIdx = this.items.findIndex(
          obj => obj.fam_no === itemObj.fam_no
        );
        this.$set(this.items, itemIdx, itemObj);
      }
    },
    async insertFamObj(insertItems) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.famInfo,
        {
          Action: `INSERT`,
          FamArray: JSON.stringify(insertItems),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        // do nothing
        return;
      }

      this.items = [...this.items, ...insertItems];
    },
  },
  created() {},
  mounted() {},
  computed: {
    fields() {
      return [
        { key: `action`, label: `` },
        { key: `fam_no`, label: `戶號`, sortable: true },
        { key: `rec_user`, label: `登錄人員編號`, sortable: true },
        { key: `rec_name`, label: `登錄人員`, sortable: true },
        { key: `rec_status`, label: `登錄人員狀態`, sortable: true },
        { key: `adi_user`, label: `審核人員編號`, sortable: true },
        { key: `adi_name`, label: `審核人員`, sortable: true },
        { key: `adi_status`, label: `審核人員狀態`, sortable: true },
        { key: `state`, label: `戶號狀態` },
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
