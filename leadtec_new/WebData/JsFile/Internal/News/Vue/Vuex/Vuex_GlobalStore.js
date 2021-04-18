import Vue from 'vue';
import Vuex from 'vuex';
import UtilFn from '../../../Common/MixinsFunctions.js';
import UtilData from '../../../Common/MixinsDataModel.js';

Vue.use(Vuex); // regist Vuex to Vue
/* eslint-disable no-undef, no-param-reassign, camelcase */
const vueStore = new Vuex.Store({
  modules: {},
  state: {
  	languageState: UtilFn.mixinLanguage(),
  	menuAry: UtilData.mixinMenuAry,
  },
  mutations: {
  },
  actions: {
  },
  getters: {},
});
/* eslint-disable no-undef, no-param-reassign, camelcase */
export default vueStore;
