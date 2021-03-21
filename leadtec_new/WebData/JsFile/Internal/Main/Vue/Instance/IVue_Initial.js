import Vue from 'vue';
import { mapActions, mapState } from 'vuex';
import BootstrapVue from 'bootstrap-vue';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import vueStore from '../Vuex/Vuex_GlobalStore';
import '../Mixins/Vue_GlobalMixins';

Vue.use(BootstrapVue);

function IVueInitialCreator() {
  const _this = this;
  let _Instance = null;
  this.Intital = () => {
    _Instance = new Vue({

      // component
      store: vueStore,
      el: '#vue-instance',
      components: {
      },
      data: {
        menu: [
          {
            key: "comp_intro",
          },
          {
            key: "news",
          },
          {
            key: "product_intro",
          },
          {
            key: "download",
          },
          {
            key: "contact",
          },
          {
            key: "home",
          },
        ],
      },
      methods: {
      },
      updated() {},
      computed: {
        ...mapState(['languageState']),
      },
      created() {},
      async mounted() {
      },
      watch: {},
      beforeDestroy() {},
    });
  };

  this._getColsure = () => {
    return { _this, _Instance };
  };
}

(function initIVue(window) {
  window.Leadtec = window.Leadtec || {};
  window.Leadtec.js_Vue_Instance = window.Leadtec.js_Vue_Instance || {};
  window.Leadtec.js_Vue_Instance.IVue_Initial = new IVueInitialCreator();
})(window);
