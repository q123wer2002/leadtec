import Vue from 'vue';
import Vuex from 'vuex';
import languagePackage from '../../../Common/language/package';

Vue.use(Vuex); // regist Vuex to Vue

const vueStore = new Vuex.Store({
  state: {
    languageCode: 'en',
    languagePackage,
  },
  mutations: {
    /* eslint-disable no-param-reassign */
    changeLanguageCode(state, newLanguageCode) {
      if (
        Object.keys(state.languagePackage).includes(newLanguageCode) === false
      ) {
        state.languageCode = `en`;
        return;
      }

      state.languageCode = newLanguageCode;
    },
    /* eslint-disable no-param-reassign */
  },
  getters: {
    i18n(state) {
      return state.languagePackage[state.languageCode];
    },
  },
});
export default vueStore;
