<template>
  <div
    class="ValidCode disabled-select"
    :style="`width:${width}; height:${height}`"
    @click="refreshCode"
  >
    <span
      v-for="(item, index) in codeList"
      :key="index"
      :style="getStyle(item)"
    >
      {{ item.code }}
    </span>
  </div>
</template>

<script>
export default {
  name: 'ValidCode',
  props: {
    width: {
      type: String,
      default: '150px',
    },
    height: {
      type: String,
      default: '50px',
    },
    length: {
      type: Number,
      default: 4,
    },
  },
  data() {
    return {
      codeList: [],
    };
  },
  mounted() {
    this.createdCode();
  },
  methods: {
    refreshCode() {
      this.createdCode();
    },
    createdCode() {
      const len = this.length;
      const codeList = [];
      const chars = 'ABCDEFGHJKMNPQRSTWXYZabcdefhijkmnprstwxyz0123456789';
      const charsLen = chars.length;
      const upperList = [`C`, `K`, `O`, `P`, `S`, `V`, `X`, `Z`];

      // 生成
      for (let i = 0; i < len; i++) {
        const rgb = [
          Math.round(Math.random() * 220),
          Math.round(Math.random() * 240),
          Math.round(Math.random() * 200),
        ];

        let code = chars.charAt(Math.floor(Math.random() * charsLen));
        const codeIdx = upperList.indexOf(code.toUpperCase());
        code = codeIdx === -1 ? code : upperList[codeIdx];

        codeList.push({
          code,
          color: `rgb(${rgb})`,
          fontSize: `2${[Math.floor(Math.random() * 10)]}px`,
          padding: `${[Math.floor(Math.random() * 10)]}px`,
          transform: `rotate(${Math.floor(Math.random() * 90) -
            Math.floor(Math.random() * 90)}deg)`,
        });
      }
      // 指向
      this.codeList = codeList;
      // 将当前数据派发出去
      this.$emit('update:value', codeList.map(item => item.code).join(''));
    },
    getStyle(data) {
      return `color: ${data.color}; font-size: ${data.fontSize}; padding: ${
        data.padding
      }; transform: ${data.transform}`;
    },
  },
};
</script>
