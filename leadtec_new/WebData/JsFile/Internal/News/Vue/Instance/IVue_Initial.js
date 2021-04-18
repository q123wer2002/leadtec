import Vue from 'vue';
import { mapActions, mapState } from 'vuex';
import BootstrapVue from 'bootstrap-vue';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import vueStore from '../Vuex/Vuex_GlobalStore';
import '../Mixins/Vue_GlobalMixins';
import HeaderComponent from '../../../Common/Components/CVue_Header.vue';
import FooterComponent from '../../../Common/Components/CVue_Footer.vue';

Vue.use(BootstrapVue);

function IVueInitialCreator() {
  const _this = this;
  let _Instance = null;
  this.Intital = () => {
    _Instance = new Vue({

      // component
      store: vueStore,
      el: '#vue_instance',
      components: {
        HeaderComponent,
        FooterComponent
      },
      data: {
      },
      methods: {
      },
      updated() {},
      computed: {
        ...mapState(['languageState', 'menuAry']),
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
