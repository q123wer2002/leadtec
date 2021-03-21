const numeral = require('numeral');

export const Color = {
  green: `#C3E6CB`,
  red: `#F5C6CB`,
};

export function fnIsJSON(dataString) {
  try {
    JSON.parse(dataString);
  } catch (e) {
    return false;
  }
  return true;
}

export function fnDeepClone(jsonObject) {
  return JSON.parse(JSON.stringify(jsonObject));
}

// only accrpt Chinese, English and number
export function fnValidInputText(textString) {
  const checker = /^[a-zA-Z0-9\u4e00-\u9fa5]+$/;
  return checker.test(textString);
}

export function fnMSToDateTime(millionSecond) {
  const leftPad = num => {
    let tempString = num.toString();
    while (tempString.length < 2) {
      tempString = `0${tempString}`;
    }
    return tempString;
  };

  const date = new Date(millionSecond);
  const dateString = [
    date.getFullYear(),
    leftPad(date.getMonth() + 1),
    leftPad(date.getDate()),
  ].join(`/`);
  const timeString = [
    leftPad(date.getHours()),
    leftPad(date.getMinutes()),
    leftPad(date.getSeconds()),
  ].join(`:`);
  return `${dateString} ${timeString}`;
}

export function fnNumToPercent(number) {
  if (number === 1) {
    return `100%`;
  }

  return `${numeral(number * 100).format(`0.0`)}%`;
}

/* eslint-disable */
export const debounce = (func, delay, isImmediate = false) => {
  let timer = null;
  return function () {
    // set context
    const context = this; const
      args = arguments;

    // set do function
    const later = function () {
      timer = null;
      if (isImmediate === false) {
        func.apply(context, args);
      }
    };

    // check is call first
    const isCallNow = (isImmediate && !timer);

    // set timer
    clearTimeout(timer);
    timer = setTimeout(later, delay);

    if (isCallNow) {
      func.apply(context, args);
    }
  };
};
/* eslint-disable */
