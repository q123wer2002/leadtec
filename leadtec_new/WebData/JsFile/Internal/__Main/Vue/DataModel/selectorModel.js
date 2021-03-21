const fnGetYearOpts = () => {
  const startYear = 103 + 1911;
  const curtYear = new Date().getFullYear();
  const yearOpts = [];

  for (let i = startYear; i <= curtYear; i++) {
    yearOpts.push(i);
  }

  return yearOpts.map(year => year - 1911);
};

export const incomeDataModel = [
  {
    key: `date`,
    text: `年月`,
    type: {
      year: `select`,
      month: `select`,
    },
    value: {
      year: 108,
      month: 7,
    },
    source: {
      year: fnGetYearOpts().map(year => {
        return {
          value: year,
          text: `${year}年`,
        };
      }),
      month: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(month => {
        return {
          value: month,
          text: `${month}月`,
        };
      }),
    },
    valid: {
      year: () => {
        return true;
      },
      month: () => {
        return true;
      },
    },
    required: true,
  },
  {
    key: `port`,
    text: `戶號`,
    type: {
      start: `number`,
      end: `number`,
    },
    value: {
      start: ``,
      end: ``,
    },
    source: {
      start: null,
      end: null,
    },
    valid: {
      start: () => {
        return true;
      },
      end: () => {
        return true;
      },
    },
  },
  {
    key: `loginman`,
    text: `登錄人員`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `reviewman`,
    text: `審核人員`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `status`,
    text: `資料狀態`,
    type: {
      code: `select`,
    },
    value: {
      code: 0,
    },
    source: {
      code: [
        {
          value: 0,
          text: `全部`,
        },
        {
          value: 1,
          text: `處理中`,
        },
        {
          value: 2,
          text: `已登錄`,
        },
        {
          value: 3,
          text: `已審核`,
        },
      ],
    },
    valid: {
      code: () => {
        return true;
      },
    },
  },
];

export const subjectModel = [
  {
    key: `subjectCode`,
    text: `科目代碼`,
    type: {
      code_no: `text`,
    },
    value: {
      code_no: ``,
    },
    source: {
      code_no: null,
    },
    valid: {
      code_no: () => {
        return true;
      },
    },
  },
  {
    key: `subjectName`,
    text: `科目名稱`,
    type: {
      code_name: `text`,
    },
    value: {
      code_name: ``,
    },
    source: {
      code_name: null,
    },
    valid: {
      code_name: () => {
        return true;
      },
    },
  },
];

export const detailedModel = [
  {
    key: `date`,
    text: `年月`,
    type: {
      year: `select`,
      month: `select`,
    },
    value: {
      year: 108,
      month: 7,
    },
    source: {
      year: fnGetYearOpts().map(year => {
        return {
          value: year,
          text: `${year}年`,
        };
      }),
      month: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(month => {
        return {
          value: month,
          text: `${month}月`,
        };
      }),
    },
    valid: {
      year: value => {
        return value > 0 || value.length > 0;
      },
      month: value => {
        return value > 0 || value.length > 0;
      },
    },
    required: true,
  },
  {
    key: `port`,
    text: `戶號`,
    type: {
      num: `text`,
    },
    value: {
      num: ``,
    },
    source: {
      num: {
        type: `dynamic`,
        api: `myFamNo`,
        key: `fam_no`,
      },
    },
    valid: {
      num: value => {
        return value > 0 || value.length > 0;
      },
    },
    required: true,
  },
  {
    key: `duration`,
    text: `期間`,
    type: {
      start: `text`,
      end: `text`,
    },
    value: {
      start: ``,
      end: ``,
    },
    source: {
      start: [
        `00`,
        `01`,
        `02`,
        `03`,
        `04`,
        `05`,
        `06`,
        `07`,
        `08`,
        `09`,
        `10`,
        `11`,
        `12`,
        `13`,
        `14`,
        `15`,
        `16`,
        `17`,
        `18`,
        `19`,
        `20`,
        `21`,
        `22`,
        `23`,
        `24`,
        `25`,
        `26`,
        `27`,
        `28`,
        `29`,
        `30`,
        `31`,
      ],
      end: [
        `00`,
        `01`,
        `02`,
        `03`,
        `04`,
        `05`,
        `06`,
        `07`,
        `08`,
        `09`,
        `10`,
        `11`,
        `12`,
        `13`,
        `14`,
        `15`,
        `16`,
        `17`,
        `18`,
        `19`,
        `20`,
        `21`,
        `22`,
        `23`,
        `24`,
        `25`,
        `26`,
        `27`,
        `28`,
        `29`,
        `30`,
        `31`,
      ],
    },
    valid: {
      start: () => {
        return true;
      },
      end: value => {
        return value > 0;
      },
    },
  },
  {
    key: `subjectCode`,
    text: `科目代碼`,
    type: {
      code_no: `text`,
    },
    value: {
      code_no: ``,
    },
    source: {
      code_no: null,
    },
    valid: {
      code_no: value => {
        return value > 0;
      },
    },
  },
  {
    key: `subjectName`,
    text: `科目名稱`,
    type: {
      code_name: `text`,
    },
    value: {
      code_name: ``,
    },
    source: {
      code_name: null,
    },
    valid: {
      code_name: value => {
        return value.length > 0;
      },
    },
  },
];

export const dataCheckerModel = [
  {
    key: `date`,
    text: `年月`,
    type: {
      year: `select`,
      month: `select`,
    },
    value: {
      year: 108,
      month: 7,
    },
    source: {
      year: fnGetYearOpts().map(year => {
        return {
          value: year,
          text: `${year}年`,
        };
      }),
      month: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(month => {
        return {
          value: month,
          text: `${month}月`,
        };
      }),
    },
    valid: {
      year: value => {
        return value > 0 || value.length > 0;
      },
      month: value => {
        return value > 0 || value.length > 0;
      },
    },
    action: 'refreshCheckTime',
    required: true,
  },
  {
    key: `port`,
    text: `戶號`,
    type: {
      start: `number`,
      end: `number`,
    },
    value: {
      start: ``,
      end: ``,
    },
    source: {
      start: null,
      end: null,
    },
    valid: {
      start: () => {
        return true;
      },
      end: () => {
        return true;
      },
    },
  },
  {
    key: `checktime`,
    text: `檢誤時間`,
    type: {
      num: `select`,
    },
    value: {
      num: ``,
    },
    source: {
      num: [],
    },
    valid: {
      num: value => {
        return value > 0 || value.length > 0;
      },
    },
    required: false,
  },
  {
    key: `checkType`,
    text: `檢誤類別`,
    type: {
      code: `select`,
    },
    value: {
      code: 0,
    },
    source: {
      code: {
        type: `dynamic2`,
        api: `paramArray`,
        filter: dataArray => {
          return [
            {
              value: 0,
              text: '全部',
            },
            ...dataArray
              .filter(obj => obj.par_typ === 'H')
              .map(obj => {
                return {
                  value: obj.par_no,
                  text: obj.par_name,
                };
              }),
          ];
        },
      },
    },
    valid: {
      code: () => {
        return true;
      },
    },
  },
];

export const portPackageModel = [
  {
    key: `date`,
    text: `年月`,
    type: {
      year: `select`,
      month: `select`,
    },
    value: {
      year: 108,
      month: 7,
    },
    source: {
      year: fnGetYearOpts().map(year => {
        return {
          value: year,
          text: `${year}年`,
        };
      }),
      month: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(month => {
        return {
          value: month,
          text: `${month}月`,
        };
      }),
    },
    valid: {
      year: value => {
        return value > 0 || value.length > 0;
      },
      month: value => {
        return value > 0 || value.length > 0;
      },
    },
    required: true,
  },
  {
    key: `port`,
    text: `戶號`,
    type: {
      start: `number`,
      end: `number`,
    },
    value: {
      start: ``,
      end: ``,
    },
    source: {
      start: null,
      end: null,
    },
    valid: {
      start: () => {
        return true;
      },
      end: () => {
        return true;
      },
    },
  },
  {
    key: `checkinMan`,
    text: `登錄人員`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `reviewMan`,
    text: `審核人員`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
];

export const accountModel = [
  {
    key: `account`,
    text: `登入帳號`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `name`,
    text: `姓名`,
    type: {
      value: `text`,
    },
    value: {
      value: ``,
    },
    source: {
      value: null,
    },
    valid: {
      value: () => {
        return true;
      },
    },
  },
  {
    key: `status`,
    text: `資料狀態`,
    type: {
      code: `select`,
    },
    value: {
      code: -1,
    },
    source: {
      code: [
        {
          value: -1,
          text: `全部`,
        },
        {
          value: 0,
          text: `停用`,
        },
        {
          value: 1,
          text: `啟用`,
        },
        {
          value: 2,
          text: `鎖定`,
        },
      ],
    },
    valid: {
      code: () => {
        return true;
      },
    },
  },
];

export const checkinPortModel = [
  {
    key: `loginman`,
    text: `登錄人員編號`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `reviewman`,
    text: `審核人員編號`,
    type: {
      id: `text`,
    },
    value: {
      id: ``,
    },
    source: {
      id: null,
    },
    valid: {
      id: () => {
        return true;
      },
    },
  },
  {
    key: `port`,
    text: `戶號`,
    type: {
      start: `number`,
      end: `number`,
    },
    value: {
      start: ``,
      end: ``,
    },
    source: {
      start: null,
      end: null,
    },
    valid: {
      start: () => {
        return true;
      },
      end: () => {
        return true;
      },
    },
  },
];
