import Vue from 'vue';
import MininsDataModel from '../../../Common/MixinsDataModel';
import MininsFunctions from '../../../Common/MixinsFunctions';

Vue.mixin({
  data() {
    return {
      ...MininsDataModel,
    };
  },
  methods: {
    ...MininsFunctions,
  },
  computed: {},
  created() {},
});
