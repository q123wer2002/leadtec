<template>
  <header id="header">
      <div class="inner">
          <nav id="nav">
              <a v-for="mm in menuAry" :href="mm.link">
                  {{languageState[mm.text_key]}}
              </a>
          </nav>
          <a href="#navPanel" class="navPanelToggle"><span class="fa fa-bars"></span></a>
      </div>
  </header>
</template>

<script>
import { mapActions, mapState } from 'vuex';

export default {
  name: 'Header',
  components: {},
  props: {},
  data() {
    return {
    };
  },
  methods: {
  },
  created() {},
  mounted() {
  },
  computed: {
    ...mapState(['languageState', 'menuAry'])
  },
  watch: {},
};
</script>

<style scoped>
  #header {
    color: #fff;
    cursor: default;
    height: 7.25em;
    left: 0;
    line-height: 3.25em;
    text-align: right;
    position: relative;
    top: 0;
    width: 100%;
    z-index: 10001;
    background: #656575;
    padding: 2em 0 2em 0;
  }

    #header .inner {
      margin: 0 auto;
      position: relative;
    }

    #header .logo {
      color: #ffffff;
      display: inline-block;
      font-weight: 400;
      height: inherit;
      left: 0;
      line-height: inherit;
      margin: 0;
      padding: 0;
      position: absolute;
      top: 0;
      font-size: 1em;
    }

      #header .logo strong {
        color: #ffffff;
        font-weight: 600;
      }

    #header a {
      -moz-transition: color 0.2s ease-in-out;
      -webkit-transition: color 0.2s ease-in-out;
      -ms-transition: color 0.2s ease-in-out;
      transition: color 0.2s ease-in-out;
      display: inline-block;
      padding: 0 0.75em;
      color: inherit;
      text-decoration: none;
      font-size: 1em;
    }

      #header a:hover {
        color: #ffffff;
        text-decoration: underline;
      }

      #header a:last-child {
        padding-right: 0;
      }

      #header a.navPanelToggle {
        display: none;
        text-decoration: none;
        height: 4em;
        width: 4em;
        z-index: 10003;
      }

        #header a.navPanelToggle .fa {
          font-size: 1.25em;
        }

        @media screen and (max-width: 980px) {

          #header a.navPanelToggle {
            display: inline-block;
          }

        }

      @media screen and (max-width: 736px) {

        #header a {
          padding: 0 0.5em;
        }

      }

  @media screen and (max-width: 980px) {

    #header {
      top: 1em;
    }

  }

  @media screen and (max-width: 736px) {

    #header {
      top: .5em;
    }

  }

  @media screen and (max-width: 480px) {

    #header {
      font-size: .9em;
    }

  }
</style>
