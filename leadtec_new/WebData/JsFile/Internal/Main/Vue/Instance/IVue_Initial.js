import Vue from 'vue';
import { mapActions, mapState } from 'vuex';
import BootstrapVue from 'bootstrap-vue';
import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import vueStore from '../Vuex/Vuex_GlobalStore';
import '../Mixins/Vue_GlobalMixins';
import BicycleIcon from 'vue-material-design-icons/Bicycle.vue';
import ProfessionalHexagonIcon from 'vue-material-design-icons/ProfessionalHexagon.vue';
import CogIcon from 'vue-material-design-icons/Cog.vue';

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
        BicycleIcon,
        ProfessionalHexagonIcon,
        CogIcon
      },
      data: {
        menuAry: [
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
        featureAry: [
          {
            icon: "BicycleIcon",
            title: "banner_1_title",
            detail: "banner_1_detail",
          },
          {
            icon: "ProfessionalHexagonIcon",
            title: "banner_2_title",
            detail: "banner_2_detail",
          },
          {
            icon: "CogIcon",
            title: "banner_3_title",
            detail: "banner_3_detail",
          },
        ],
        introMainAry: [
          {
            title: "intro_1_title",
            picAry: [
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile001.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile002.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile003.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile007.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile008.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile009.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile010.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile011.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile012.jpg",
            ],
          },
          {
            title: "intro_2_title",
            picAry: [
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile004.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile005.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile006.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile013.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile014.jpg",
              "http://www.leadtec.com.tw/upload/Image/main/companyprofile015.jpg",
            ],
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
