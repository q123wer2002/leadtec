<template>
  <div id="mainPage">
    <h5>戶口組成資料</h5>
    <selector :filterModel="selectorModel" @search="searchEvent"></selector>

    <!-- filtered data -->
    <div id="tableData">
      <b-button-group class="my-3 float-sm-left" size="sm">
        <b-button @click="selectAllRows">全選</b-button>
        <b-button @click="clearSelected">取消全選</b-button>
      </b-button-group>
      <div class="w-25 d-inline-block justify-content-center">
        <label class="d-inline-block">每頁顯示筆數</label>
        <b-form-input
          v-model="perPage"
          type="number"
          size="sm"
          class="d-inline-block"
        ></b-form-input>
      </div>
      <b-button-group class="my-3 float-sm-right" size="sm">
        <b-button variant="info" @click="openUploader" :disabled="isImporting">
          <b-spinner small v-if="isImporting"></b-spinner>
          <span v-else>資料匯入</span>
        </b-button>
        <b-button
          variant="info"
          :disabled="!isSelectedItems"
          @click="exportCSV"
        >
          資料匯出
        </b-button>
        <b-button
          variant="info"
          :disabled="!isSelectedItems"
          @click="deleteItems"
        >
          刪除
        </b-button>
      </b-button-group>

      <b-table
        ref="domDatatable"
        :busy="isBusy"
        :items="items"
        :fields="fields"
        :per-page="perPage"
        :current-page="currentPage"
        selectable
        select-mode="multi"
        selected-variant="info"
        @row-selected="onRowSelected"
        hover
        small
        head-variant="dark"
      >
        <div slot="table-busy" class="text-center text-danger my-2">
          <b-spinner class="align-middle"></b-spinner>
          <strong>Loading...</strong>
        </div>
        <template slot="[selected]" slot-scope="{ rowSelected, index }">
          <b-form-checkbox
            v-model="rowSelected"
            @change="selectOneItem(index)"
          ></b-form-checkbox>
        </template>
        <template slot="[state]" slot-scope="{ item }">
          <span>{{ statusToString(item.state) }}</span>
        </template>
        <template slot="[btnFamilyPackage]" slot-scope="data">
          <a href="javascript:;" @click="openComponent('MainCard', data.item)">
            主卡
          </a>
          |
          <a
            href="javascript:;"
            @click="openComponent('FamilyMember', data.item)"
          >
            組成
          </a>
          |
          <a href="javascript:;" @click="openComponent('HouseInfo', data.item)">
            住宅
          </a>
          |
          <a
            href="javascript:;"
            @click="openComponent('HomeEquipment', data.item)"
          >
            家庭設備
          </a>
          |
          <a href="javascript:;" @click="openComponent('Education', data.item)">
            在學
          </a>
          |
          <a
            href="javascript:;"
            @click="openComponent('StaticPayment', data.item)"
          >
            固定支出
          </a>
        </template>
      </b-table>

      <b-pagination
        v-model="currentPage"
        :total-rows="items.length"
        :per-page="perPage"
        size="sm"
        class="justify-content-center"
      ></b-pagination>

      <b-modal ref="domModal" size="xl" hide-footer>
        <component
          :is="selectComponent"
          :coFamData="selectedCoFamData"
          @updateFamItem="updateFamData"
          style="width: auto; overflow-y: auto;"
        ></component>
      </b-modal>
      <input
        type="file"
        accept=".csv"
        ref="domFileInput"
        @change="importCSV"
        style="display: none;"
      />
    </div>
  </div>
</template>

<script>
import { statusMapToString } from '../DataModel/dataModel.js';
import { portPackageModel } from '../DataModel/selectorModel.js';
import Selector from './CVue_Selector.vue';
import MainCard from './PortPackage/MainCard.vue';
import HouseInfo from './PortPackage/HouseInfo.vue';
import HomeEquipment from './PortPackage/HomeEquipment.vue';
import Education from './PortPackage/Education.vue';
import StaticPayment from './PortPackage/StaticPayment.vue';
import FamilyMember from './PortPackage/FamilyMember.vue';

export default {
  /* eslint-disable no-undef, no-param-reassign, camelcase, no-restricted-globals, no-await-in-loop */
  name: 'PortCardMaintain',
  components: {
    Selector,
    MainCard,
    HouseInfo,
    HomeEquipment,
    Education,
    StaticPayment,
    FamilyMember,
  },
  props: {},
  data() {
    return {
      selectorModel: portPackageModel,
      selectComponent: ``,
      selectedCoFamData: {},

      queryObject: {},
      isSelectAll: false,
      isImporting: false,

      fields: [],
      items: [],
      memItems: [],
      currentPage: 1,
      perPage: 20,
      selected: [],
      isBusy: false,
    };
  },
  methods: {
    async searchEvent(filterObject) {
      const { date, checkinMan, port, reviewMan } = filterObject;
      this.queryObject = {};

      // add date
      if (date.year !== 0 && date.month !== 0) {
        this.queryObject.Year = date.year;
        this.queryObject.Month = date.month;
      }

      // add checkinMan
      if (checkinMan && checkinMan.id.length !== 0) {
        this.queryObject.RecUser = checkinMan.id;
      }

      // add port
      if (
        port.end.length !== 0 &&
        port.start.length !== 0 &&
        port.end >= port.start
      ) {
        this.queryObject.FamNoStart = port.start;
        this.queryObject.FamNoEnd = port.end;
      }

      if (reviewMan && reviewMan.id.length !== 0) {
        this.queryObject.AdiUser = reviewMan.id;
      }

      await this.queryFamilyCardData(this.queryObject);
    },
    onRowSelected(items) {
      this.selected = items;
    },
    selectAllRows() {
      this.$refs.domDatatable.selectAllRows();
      this.isSelectAll = true;
    },
    clearSelected() {
      this.$refs.domDatatable.clearSelected();
      this.isSelectAll = false;
    },
    selectOneItem(index) {
      const isSelected = this.$refs.domDatatable.isRowSelected(index);
      if (isSelected) {
        this.$refs.domDatatable.unselectRow(index);
        return;
      }

      this.$refs.domDatatable.selectRow(index);
    },
    openUploader() {
      this.$refs.domFileInput.value = ``;
      this.$refs.domFileInput.click();
    },
    parseToEnoughDigital(string, nDigital) {
      let tempNewValue = string;
      while (tempNewValue.length < nDigital) {
        tempNewValue = `0${tempNewValue}`;
      }

      return tempNewValue;
    },
    importCSV(event) {
      const inputFiles = event.target;

      const reader = new FileReader();
      reader.onload = async () => {
        const fileTxt = reader.result;
        if (fileTxt.length === 0) {
          alert(`選取的檔案沒有資料`);
          return;
        }

        this.isImporting = true;
        const dataArray = fileTxt
          .split(`\n`)
          .slice(1)
          .map(dataString => {
            const dataSubData = dataString.split(`,`);
            const tempArray = [];
            let startIdx = 0;
            let tempData = ``;
            let isMerge = false;
            while (startIdx !== dataSubData.length) {
              if (
                dataSubData[startIdx].startsWith(`"`) &&
                dataSubData[startIdx].endsWith(`"`) === false &&
                isMerge === false
              ) {
                isMerge = true;
                tempData += dataSubData[startIdx].substring(
                  dataSubData[startIdx].length,
                  1
                );
              } else if (
                dataSubData[startIdx].endsWith(`"`) &&
                isMerge === true
              ) {
                tempData += `,${dataSubData[startIdx].substring(
                  0,
                  dataSubData[startIdx].length - 1
                )}`;
                // console.log(parseInt(tempData.replace(',', ''), 10));
                tempArray.push(JSON.parse(JSON.stringify(tempData)));

                tempData = ``;
                isMerge = false;
              } else if (isMerge) {
                tempData += `,${dataSubData[startIdx]}`;
              } else {
                tempArray.push(dataSubData[startIdx]);
              }

              startIdx += 1;
            }

            if (
              tempArray.length !== 126 ||
              tempArray[0].length === 0 ||
              tempArray[1].length === 0
            ) {
              return {};
            }

            const fam_no = `${this.parseToEnoughDigital(
              tempArray[3],
              5
            )}${this.parseToEnoughDigital(tempArray[4], 3)}`;

            return {
              fam: {
                ie_year: tempArray[0],
                ie_mon: tempArray[1],
                fam_no,
                cou_name: tempArray[2],
                cou_no: tempArray[3],
                sam_seq: tempArray[4],
                fam_head: tempArray[5],
                fam_addr: tempArray[6],
                phone: tempArray[7],
                sam_no: tempArray[8],
                int_name: tempArray[9],
                adi_name: tempArray[10],
                sor_name: tempArray[11],
                rec_name: tempArray[12],
                fam_cnt: tempArray[13],
                job_cnt: tempArray[14],
                ie_lev: tempArray[15],
                fam_cha: tempArray[16],
                hou_own: tempArray[51],
                hou_loan: tempArray[52],
                hou_use: tempArray[53],
                hou_typ: tempArray[54],
                tap_wat: tempArray[55],
                par_none: tempArray[56],
                par_self: tempArray[57],
                par_rent: tempArray[58],
                hou_land: tempArray[59],
                hou_ping: tempArray[60],
                hou_man_fee: tempArray[61],
                col_tv: tempArray[62],
                led_tv: tempArray[63],
                dvd_palyer: tempArray[64],
                audio: tempArray[65],
                computer: tempArray[66],
                wash_mach: tempArray[67],
                telphone: tempArray[68],
                car: tempArray[69],
                moto: tempArray[70],
                dig_camera: tempArray[71],
                wat_heater: tempArray[72],
                vedio: tempArray[73],
                tv_game: tempArray[74],
                mul_vedio: tempArray[75],
                camera: tempArray[76],
                piano: tempArray[77],
                cable_tv: tempArray[78],
                mobile: tempArray[79],
                ind_cooker: tempArray[80],
                air_cond: tempArray[81],
                dehumidifier: tempArray[82],
                dryer: tempArray[83],
                air_puri: tempArray[84],
                wat_filter: tempArray[85],
                vac_cleaner: tempArray[86],
                dri_mach: tempArray[87],
                mic_oven: tempArray[88],
                newspaper: tempArray[89],
                magazine: tempArray[90],
                nursery: tempArray[91],
                elementary: tempArray[92],
                junior: tempArray[93],
                senior: tempArray[94],
                college: tempArray[95],
                baby: tempArray[96],
                wat_fee: tempArray[97],
                ele_fee: tempArray[98],
                int_tel_fee: tempArray[99],
                mob_fee: tempArray[100],
                net_fee: tempArray[101],
                gas_fee: tempArray[102],
                cab_fee: tempArray[103],
                mul_vedio_fee: tempArray[104],
                hou_insu: tempArray[105],
                hou_sec_fee: tempArray[106],
                par_rent_fee: tempArray[107],
                nanny: tempArray[108],
                driver: tempArray[109],
                rent: tempArray[110],
                rent_land: tempArray[111],
                dorm_fee: tempArray[112],
                hou_loan_val: tempArray[113],
                hou_own_val: tempArray[114],
                dorm_sch_fee: tempArray[115],
                commute: tempArray[116],
                int_user: tempArray[117],
                adi_user: tempArray[118],
                sor_user: tempArray[119],
                rec_user: tempArray[120],
                fam_remark: tempArray[121]
                  ? tempArray[121].replace(`'`, `''`)
                  : '',
                tutoring: tempArray[123],
                hou_tax: isNaN(parseInt(tempArray[124], 10))
                  ? 0
                  : parseInt(tempArray[124], 10),
                land_tax: isNaN(parseInt(tempArray[125], 10))
                  ? 0
                  : parseInt(tempArray[125], 10),
              },
              fammem: {
                ie_year: tempArray[0],
                ie_mon: tempArray[1],
                fam_no,
                mem_no: tempArray[17],
                mem_name: tempArray[18],
                title: tempArray[19],
                gender: tempArray[20],
                bir_year: tempArray[21],
                bir_mon: tempArray[22],
                age: tempArray[23],
                edu_no: tempArray[24],
                sch_typ: tempArray[25],
                sch_name: tempArray[26],
                education: tempArray[27],
                grade: tempArray[28],
                job_typ: tempArray[29],
                job_title: tempArray[30],
                job_com: tempArray[31],
                fam_head_rel: tempArray[32],
                job_no: tempArray[33],
                job_typ_no: tempArray[34],
                inc_fam_prc: tempArray[35],
                fee_fam_prc: tempArray[36],
                insu_pub: parseInt(tempArray[37].replace(',', ''), 10),
                insu_lab: parseInt(tempArray[38].replace(',', ''), 10),
                insu_farm: parseInt(tempArray[39].replace(',', ''), 10),
                insu_fish: parseInt(tempArray[40].replace(',', ''), 10),
                insu_mil: parseInt(tempArray[41].replace(',', ''), 10),
                insu_medi: parseInt(tempArray[42].replace(',', ''), 10),
                insu_acci: parseInt(tempArray[43].replace(',', ''), 10),
                insu_life: parseInt(tempArray[44].replace(',', ''), 10),
                insu_heal: parseInt(tempArray[45].replace(',', ''), 10),
                annuity: tempArray[46],
                insu_car: parseInt(tempArray[47].replace(',', ''), 10),
                insu_moto: parseInt(tempArray[48].replace(',', ''), 10),
                insu_soci_amt: parseInt(tempArray[49].replace(',', ''), 10),
                insu_heal_amt: parseInt(tempArray[50].replace(',', ''), 10),
                mem_remark: tempArray[122],
              },
            };
          })
          .filter(obj => Object.keys(obj).length > 0);

        let isSuccess;
        let startIdx = 0;
        let importData = [];
        const importNum = 150;
        while (startIdx <= dataArray.length) {
          const endIdx =
            dataArray.length < startIdx + importNum
              ? dataArray.length
              : startIdx + importNum;
          importData = dataArray.slice(startIdx, endIdx);

          // import into database
          isSuccess = await this.updateCoFamData(
            importData.map(obj => obj.fam),
            false
          );
          isSuccess = await this.updateCoFamMemData(
            importData.map(obj => obj.fammem),
            false
          );

          if (isSuccess === false) {
            break;
          }
          startIdx += importNum;
        }
        if (isSuccess === false) {
          alert('儲存失敗');
          this.isImporting = false;
          return;
        }

        alert('儲存成功');
        this.isImporting = false;
      };

      if (inputFiles.files.length === 0) {
        return;
      }

      reader.readAsText(inputFiles.files[0], `utf-8`);
    },
    exportCSV() {
      let detailedData = this.selected;
      if (this.isSelectAll) {
        detailedData = this.items;
      }

      const exportCSVData = [
        [
          `年別`,
          `月份`,
          `縣市村里名稱`,
          `樣本村里代號`,
          `樣本序號`,
          `戶長姓名`,
          `地址`,
          `電話號碼`,
          `樣本名冊流水號`,
          `訪問員`,
          `審核員`,
          `整理員`,
          `登錄員`,
          `戶內人數`,
          `就業人數`,
          `收入級距`,
          `本月是否換戶`,
          `戶內人口代號`,
          `姓名`,
          `稱謂`,
          `性別`,
          `出生:年`,
          `出生:月`,
          `年齡`,
          `最高教育程度編號`,
          `公私立`,
          `校名`,
          `教育程度`,
          `年級`,
          `就業別編號`,
          `擔任職務`,
          `機關全銜`,
          `與經濟戶長之關係代號`,
          `本業:職業編號`,
          `本業:行業編號`,
          `所得提供家庭(%)`,
          `費用家庭供給(%)`,
          `公保(幾個月繳納)`,
          `一般勞保(幾個月繳納)`,
          `農保(幾個月繳納)`,
          `漁會甲類會員勞保(幾個月繳納)`,
          `軍保(幾個月繳納)`,
          `健康醫療險(幾個月繳納)`,
          `意外傷害險(幾個月繳納)`,
          `定期終身壽險(幾個月繳納)`,
          `全民健保費(幾個月繳納)`,
          `國民年金(幾個月繳納)`,
          `汽車保險費(幾個月繳納)`,
          `機車保險費(幾個月繳納)`,
          `社會保險繳納金額`,
          `健保繳納金額`,
          `住宅所屬`,
          `現住自宅房屋貸款`,
          `用途`,
          `建築式樣`,
          `自來水設備`,
          `有車者之停車位:無`,
          `有車者之停車位:自有`,
          `有車者之停車位:租借`,
          `住宅面積:佔地`,
          `住宅面積:建坪`,
          `住宅管理費`,
          `彩色電視機`,
          `液晶`,
          `電漿電視機`,
          `數位影音光碟機`,
          `音響`,
          `家用電腦`,
          `洗衣機`,
          `電話機`,
          `汽車`,
          `機車`,
          `數位相機`,
          `熱水器  錄放影機`,
          `電視遊樂器`,
          `多媒體隨選視訊`,
          `攝影機`,
          `鋼琴(含電子琴)`,
          `有線電視頻道設備(含小耳朵)`,
          `行動電話`,
          `電磁爐`,
          `冷暖氣機`,
          `除濕機`,
          `烘衣機`,
          `空氣清靜機`,
          `濾水器`,
          `吸塵器`,
          `開飲機`,
          `微波爐`,
          `報紙`,
          `期刊雜誌`,
          `幼兒園`,
          `國小`,
          `國中`,
          `高中`,
          `專科以上`,
          `嬰兒由媬姆照顧`,
          `水費`,
          `電費`,
          `室內電話費`,
          `行動電話費`,
          `網路月租費`,
          `天然瓦斯費`,
          `有線電視月租費`,
          `多媒體隨選視訊月租費`,
          `住宅(火險、竊盜險等)`,
          `住宅管理保全費`,
          `停車位租用費`,
          `保姆費`,
          `佣人司機費`,
          `實付房租`,
          `實付地租`,
          `學生就學住宿費`,
          `配(借)住設算`,
          `自有自住設算`,
          `學生住校住宿費`,
          `通勤費`,
          `訪問員編號`,
          `審核員編號`,
          `整理員編號`,
          `登錄員編號`,
          `主卡備註`,
          `戶口組成卡備註`,
          `補習費`,
          `房屋稅`,
          `地價稅`,
        ], // column name
      ];

      // loop to get data
      for (let i = 0; i < detailedData.length; i++) {
        const {
          ie_year,
          ie_mon,
          fam_no,
          cou_name,
          cou_no,
          sam_seq,
          fam_head,
          fam_addr,
          phone,
          sam_no,
          int_name,
          adi_name,
          sor_name,
          rec_name,
          fam_cnt,
          job_cnt,
          ie_lev,
          fam_cha,
          hou_own,
          hou_loan,
          hou_use,
          hou_typ,
          tap_wat,
          par_none,
          par_self,
          par_rent,
          hou_land,
          hou_ping,
          hou_man_fee,
          col_tv,
          led_tv,
          dvd_palyer,
          audio,
          computer,
          wash_mach,
          telphone,
          car,
          moto,
          dig_camera,
          wat_heater,
          vedio,
          tv_game,
          mul_vedio,
          camera,
          piano,
          cable_tv,
          mobile,
          ind_cooker,
          air_cond,
          dehumidifier,
          dryer,
          air_puri,
          wat_filter,
          vac_cleaner,
          dri_mach,
          mic_oven,
          newspaper,
          magazine,
          nursery,
          elementary,
          junior,
          senior,
          college,
          baby,
          wat_fee,
          ele_fee,
          int_tel_fee,
          mob_fee,
          net_fee,
          gas_fee,
          cab_fee,
          mul_vedio_fee,
          hou_insu,
          hou_sec_fee,
          par_rent_fee,
          nanny,
          driver,
          rent,
          rent_land,
          dorm_fee,
          hou_loan_val,
          hou_own_val,
          dorm_sch_fee,
          commute,
          int_no,
          adi_no,
          sor_no,
          rec_no,
          fam_remark,
          tutoring,
          hou_tax,
          land_tax,
        } = detailedData[i];
        const famNoMemData = this.memItems.filter(obj => obj.fam_no === fam_no);

        for (let j = 0; j < famNoMemData.length; j++) {
          const {
            mem_no,
            mem_name,
            title,
            gender,
            bir_year,
            bir_mon,
            age,
            edu_no,
            sch_typ,
            sch_name,
            education,
            grade,
            job_typ,
            job_title,
            job_com,
            fam_head_rel,
            job_no,
            job_typ_no,
            inc_fam_prc,
            fee_fam_prc,
            insu_pub,
            insu_lab,
            insu_farm,
            insu_fish,
            insu_mil,
            insu_medi,
            insu_acci,
            insu_life,
            insu_heal,
            annuity,
            insu_car,
            insu_moto,
            insu_soci_amt,
            insu_heal_amt,
            mem_remark,
          } = famNoMemData[j];

          const dataArray = [
            ie_year,
            ie_mon,
            cou_name,
            cou_no,
            sam_seq,
            fam_head,
            fam_addr,
            phone,
            sam_no,
            int_name,
            adi_name,
            sor_name,
            rec_name,
            fam_cnt,
            job_cnt,
            ie_lev,
            fam_cha,
            mem_no,
            mem_name,
            title,
            gender,
            bir_year,
            bir_mon,
            age,
            edu_no,
            sch_typ,
            sch_name,
            education,
            grade,
            job_typ,
            job_title,
            job_com,
            fam_head_rel,
            job_no,
            job_typ_no,
            inc_fam_prc,
            fee_fam_prc,
            insu_pub,
            insu_lab,
            insu_farm,
            insu_fish,
            insu_mil,
            insu_medi,
            insu_acci,
            insu_life,
            insu_heal,
            annuity,
            insu_car,
            insu_moto,
            insu_soci_amt,
            insu_heal_amt,
            hou_own,
            hou_loan,
            hou_use,
            hou_typ,
            tap_wat,
            par_none,
            par_self,
            par_rent,
            hou_land,
            hou_ping,
            hou_man_fee,
            col_tv,
            led_tv,
            dvd_palyer,
            audio,
            computer,
            wash_mach,
            telphone,
            car,
            moto,
            dig_camera,
            wat_heater,
            vedio,
            tv_game,
            mul_vedio,
            camera,
            piano,
            cable_tv,
            mobile,
            ind_cooker,
            air_cond,
            dehumidifier,
            dryer,
            air_puri,
            wat_filter,
            vac_cleaner,
            dri_mach,
            mic_oven,
            newspaper,
            magazine,
            nursery,
            elementary,
            junior,
            senior,
            college,
            baby,
            wat_fee,
            ele_fee,
            int_tel_fee,
            mob_fee,
            net_fee,
            gas_fee,
            cab_fee,
            mul_vedio_fee,
            hou_insu,
            hou_sec_fee,
            par_rent_fee,
            nanny,
            driver,
            rent,
            rent_land,
            dorm_fee,
            hou_loan_val,
            hou_own_val,
            dorm_sch_fee,
            commute,
            int_no,
            adi_no,
            sor_no,
            rec_no,
            fam_remark,
            mem_remark,
            tutoring,
            hou_tax,
            land_tax,
          ];
          exportCSVData.push(dataArray);
        }
      }

      const csvDataString = exportCSVData.map(col => col.join(`,`)).join('\n');
      const encodedUri = URL.createObjectURL(
        new Blob([`\uFEFF${csvDataString}`], {
          type: `text/csv;charset=utf-8;`,
        })
      );

      // create link
      const link = document.createElement(`a`);
      link.setAttribute(`href`, encodedUri);
      link.setAttribute(`download`, `famCoCard.csv`);
      document.body.appendChild(link);
      link.click();

      // clear
      this.clearSelected();
    },
    openComponent(componentKey, coFamData) {
      if (componentKey === `FamilyMember`) {
        const famNoMemData = this.memItems
          .filter(obj => obj.fam_no === coFamData.fam_no)
          .map(obj => {
            return {
              ...obj,
              bir_mon: parseInt(obj.bir_mon, 10),
              edu_no: parseInt(obj.edu_no, 10),
            };
          });
        this.selectedCoFamData = famNoMemData;
      } else {
        this.selectedCoFamData = coFamData;
      }

      this.selectComponent = componentKey;
      this.$refs.domModal.show();
    },
    async deleteItems() {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.familyData,
        {
          Action: `DELETE`,
          ...this.queryObject,
          FamilyDataList: JSON.stringify(this.selected),
        }
      );

      // delete items
      if (resObject.status !== this.mixinBackendErrorCode.success) {
        return;
      }

      for (let i = 0; i < this.selected.length; i++) {
        const { fam_no } = this.selected[i];
        // delete fam_mem
        const deleteItem = this.memItems.filter(obj => obj.fam_no === fam_no);
        if (this.deleteFamMemData(deleteItem) === false) {
          break;
        }

        // delete local var
        const itemIdx = this.items.findIndex(obj => obj.fam_no === fam_no);
        if (itemIdx !== -1) {
          this.$delete(this.items, itemIdx);
        }
      }
    },
    async queryFamilyCardData(queryObject) {
      this.isBusy = true;
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.familyData,
        {
          Action: `READ`,
          ...queryObject,
        }
      );

      if (resObject.status === this.mixinBackendErrorCode.success) {
        this.items = resObject.data.co_fam || [];
        this.memItems = resObject.data.co_fam_mem || [];
      }
      this.isBusy = false;
      return resObject;
    },
    async updateFamData(data) {
      if (this.selectComponent === `FamilyMember`) {
        await this.updateCoFamMemData(data);
      } else {
        await this.updateCoFamData([data]);
      }

      this.$refs.domModal.hide();
    },
    async updateCoFamMemData(data, isNeed2ShowMsg = true) {
      if (data.length === 0) {
        return false;
      }

      const { fam_no } = data[0];
      const filteredItems = this.memItems.filter(obj => obj.fam_no === fam_no);

      const deleteItems = filteredItems.filter(
        obj =>
          data.map(dataObj => dataObj.mem_no).includes(obj.mem_no) === false
      );
      const updateItems = data.filter(obj =>
        filteredItems.map(dataObj => dataObj.mem_no).includes(obj.mem_no)
      );
      const insertItems = data.filter(
        obj =>
          updateItems.map(tempObj => tempObj.mem_no).includes(obj.mem_no) ===
          false
      );

      let isSuccess = false;
      if (deleteItems.length > 0) {
        isSuccess = await this.deleteFamMemData(deleteItems);
        if (isSuccess === false) {
          if (isNeed2ShowMsg) {
            alert('儲存失敗');
          }
          return false;
        }
      }

      if (updateItems.length > 0 || insertItems.length > 0) {
        isSuccess = await this.updateFamMemData(updateItems, insertItems);
        if (isSuccess === false) {
          if (isNeed2ShowMsg) {
            alert('儲存失敗');
          }
          return false;
        }
      }

      if (isNeed2ShowMsg) {
        alert(`儲存成功`);
      }
      return true;
    },
    async updateFamMemData(updateItems, insertItems) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.familyData,
        {
          Action: `UPDATE`,
          FamMemData: JSON.stringify([...updateItems, ...insertItems]),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        return false;
      }

      for (let i = 0; i < updateItems.length; i++) {
        const { mem_no, fam_no } = updateItems[i];
        const itemIdx = this.memItems.findIndex(
          obj => obj.fam_no === fam_no && obj.mem_no === mem_no
        );
        this.$set(this.memItems, itemIdx, updateItems[i]);
      }

      this.memItems = [...this.memItems, ...insertItems];

      return true;
    },
    async deleteFamMemData(deleteItems) {
      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.familyData,
        {
          Action: `DELETE`,
          FamMemData: JSON.stringify(deleteItems),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        return false;
      }

      for (let i = 0; i < deleteItems.length; i++) {
        const { mem_no, fam_no } = deleteItems[i];
        const itemIdx = this.memItems.findIndex(
          obj => obj.fam_no === fam_no && obj.mem_no === mem_no
        );
        this.$delete(this.memItems, itemIdx);
      }

      return true;
    },
    async updateCoFamData(data, isNeed2ShowMsg = true) {
      // cannot be empty
      if (data.length === 0) {
        return false;
      }

      const resObject = await this.mixinCallBackService(
        this.mixinBackendService.familyData,
        {
          Action: `UPDATE`,
          FamData: JSON.stringify(data),
        }
      );

      if (resObject.status !== this.mixinBackendErrorCode.success) {
        if (isNeed2ShowMsg) {
          alert(`儲存失敗`);
        }
        return false;
      }

      // update local var
      const { fam_no } = data;
      const famIdx = this.items.findIndex(obj => obj.fam_no === fam_no);
      if (famIdx !== -1) {
        this.$set(this.items, famIdx, data);
      }

      if (isNeed2ShowMsg) {
        alert(`儲存成功`);
      }
      return true;
    },
  },
  created() {
    this.fields = [
      { key: `selected`, label: `勾選` },
      { key: `ie_year`, label: `年` },
      { key: `ie_mon`, label: `月` },
      { key: `cou_name`, label: `縣市村里名稱`, sortable: true },
      { key: `fam_no`, label: `戶號`, sortable: true },
      { key: `fam_head`, label: `戶長姓名` },
      { key: `fam_cnt`, label: `戶內人數` },
      { key: `job_cnt`, label: `就業人數` },
      { key: `fam_cha`, label: `是否換戶` },
      { key: `rec_user`, label: `登錄人員`, sortable: true },
      { key: `adi_user`, label: `審核人員`, sortable: true },
      { key: `state`, label: `資料狀態`, sortable: true },
      { key: `btnFamilyPackage`, label: `戶口組成` },
    ];
  },
  mounted() {},
  computed: {
    isSelectedItems() {
      if (this.selected.length === 0) {
        return false;
      }

      return this.selected.length > 0;
    },
    statusToString() {
      return statusCode => {
        return statusMapToString[statusCode];
      };
    },
  },
  watch: {
    currentPage: {
      handler() {
        this.$nextTick(() => {
          if (this.isSelectAll) {
            this.selectAllRows();
          }
        });
      },
    },
  },
  /* eslint-disable no-undef, no-param-reassign, camelcase, no-restricted-globals, no-await-in-loop */
};
</script>

<style scoped>
#mainPage {
  text-align: center;
}
#tableData {
  text-align: center;
  width: 95%;
  margin: auto;
  overflow-y: auto;
}
</style>
