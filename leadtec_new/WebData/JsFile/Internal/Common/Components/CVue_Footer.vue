<template>
	<footer id="footer" class="align-center" style="background: #DD0012;">
	    <p style="margin: 0;color: white">LEADTEC CO., LTD. 利慶工業股份有限公司</p>
	    <p style="margin: 0;color: white">&copy; 2021 LEADTEC</p>
	</footer>
</template>

<script scoped>
import { mapActions, mapState } from 'vuex';

export default {
  name: 'Footer',
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
	#footer {
		padding: 2em 0;
		background: #ff7c89;
		color: #fff;
		text-align: center;
		position: relative;
	}

	#footer h3 {
		color: #FFF;
		margin-bottom: 2em !important;
	}

	#footer label {
		text-align: left;
		color: #FFF;
	}

	#footer .copyright {
		color: rgba(255, 255, 255, 0.5);
		font-size: 0.8em;
		margin: 0 0 2em 0;
		padding: 0;
	}

		#footer .copyright a {
			color: rgba(255, 255, 255, 0.5);
			text-decoration: none;
		}

			#footer .copyright a:hover {
				color: #FFF;
			}

	@media screen and (max-width: 980px) {

		#footer {
			padding: 4em 0;
		}

	}

	@media screen and (max-width: 736px) {

		#footer {
			padding: 3em 0;
		}

	}
</style>