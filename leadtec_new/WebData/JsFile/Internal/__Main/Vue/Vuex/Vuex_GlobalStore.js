import Vue from 'vue';
import Vuex from 'vuex';
import UtilFn from '../../../Common/MixinsFunctions.js';
import UtilData from '../../../Common/MixinsDataModel.js';

Vue.use(Vuex); // regist Vuex to Vue
/* eslint-disable no-undef, no-param-reassign, camelcase */
const vueStore = new Vuex.Store({
  modules: {},
  state: {
    subjectArray: [],
    paramArray: [],
    reportArray: [],
    codeAttrArray: [],
  },
  mutations: {
    fnInitialReport(state, reportArray) {
      state.reportArray = reportArray;
    },
    fnInitialSubject(state, tempArry) {
      state.subjectArray = tempArry;
    },
    fnInsertSubject(state, subjectObject) {
      Vue.set(state.subjectArray, state.subjectArray.length, subjectObject);
    },
    fnUpdateSubject(state, object) {
      const { preSubjectObj, newSubjectObj } = object;
      const idx = state.subjectArray.findIndex(
        obj =>
          obj.code_no === preSubjectObj.code_no &&
          obj.code_name === preSubjectObj.code_name
      );

      Vue.set(state.subjectArray, idx, newSubjectObj);
    },
    fnDeleteSubject(state, deleteAry) {
      for (let i = 0; i < deleteAry.length; i++) {
        const subIdx = state.subjectArray.findIndex(
          obj =>
            obj.code_no === deleteAry[i].code_no &&
            obj.code_name === deleteAry[i].code_name
        );

        if (subIdx === -1) {
          continue;
        }

        Vue.delete(state.subjectArray, subIdx);
      }
    },
    fnSetDetaultName(state, subjectObj) {
      state.subjectArray.forEach((obj, index) => {
        if (obj.code_no === subjectObj.code_no) {
          if (obj.code_name !== subjectObj.code_name) {
            // set empty for all same code
            Vue.set(state.subjectArray[index], `def_fg`, ``);
          }
        }
      });
    },
    fnUpdateParam(state, placeArray) {
      state.paramArray = placeArray;
    },
    fnUpdateCodeAttr(state, codeAttrArray) {
      state.codeAttrArray = codeAttrArray;
    },
  },
  actions: {
    async initialReport(context) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.reporter,
        {
          Action: `GETREPORTS`,
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnInitialReport`, resObject.data || []);
      }
    },

    // for subject
    async initialSubject(context, queryObject) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.subjectData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnInitialSubject`, resObject.data || []);
      }
    },
    async updateSubject(context, queryObject) {
      const { preSubjectObj, newSubjectObj } = queryObject;
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.subjectData,
        {
          Action: `UPDATE`,
          PreSubject: JSON.stringify(preSubjectObj),
          NewSubject: JSON.stringify(newSubjectObj),
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnUpdateSubject`, { preSubjectObj, newSubjectObj });
      }
    },
    async saveSubject(context, subjectObject) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.subjectData,
        {
          Action: `INSERT`,
          Subject: JSON.stringify(subjectObject),
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnInsertSubject`, subjectObject);
      }
    },
    async deleteSubjects(context, subjectArray) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.subjectData,
        {
          Action: `DELETE`,
          SubjectArray: JSON.stringify(subjectArray),
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnDeleteSubject`, subjectArray);
      }
    },
    async setSubjectDefaultName(context, subjectObj) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.subjectData,
        {
          Action: `SETDEFAULTNAME`,
          Subject: JSON.stringify(subjectObj),
        }
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnSetDetaultName`, subjectObj);
      }
    },

    // for param
    async initialParam(context) {
      const resObject = await UtilFn.mixinCallBackService(
        UtilData.mixinBackendService.paramInfo
      );

      if (resObject.status === UtilData.mixinBackendErrorCode.success) {
        context.commit(`fnUpdateParam`, resObject.data.param || []);
        context.commit(`fnUpdateCodeAttr`, resObject.data.code_attr || []);
      }
    },
  },
  getters: {},
});
/* eslint-disable no-undef, no-param-reassign, camelcase */
export default vueStore;
