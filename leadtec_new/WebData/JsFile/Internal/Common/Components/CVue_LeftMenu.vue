<template>
  <div>
    
  </div>
</template>

<script>
import { mapActions, mapState } from 'vuex';

export default {
  name: 'LeftMenu',
  components: {},
  props: {
    title: String,
    menuItem: Array,
  },
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
  
</style>
