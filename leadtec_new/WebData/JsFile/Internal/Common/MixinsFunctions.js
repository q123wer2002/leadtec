import 'babel-polyfill';
import mixinDataModel from './MixinsDataModel.js';

/* eslint-disable no-undef */
/* eslint-disable no-unused-vars */
const { mixinBackendErrorCode } = mixinDataModel;
const mixinFuncitons = {
  // go pages
  mixinToHomePage() {
    const currentURLInfo = new URL(window.location.origin);
    window.location = `${currentURLInfo.href}${webpackDashboardName}`;
  },
  mixinToLoginPage() {
    const currentURLInfo = new URL(window.location.origin);
    window.location = `${
      currentURLInfo.href
    }${webpackDashboardName}/Login.html`;
  },

  // check status
  mixinIsBackendStatusGood(responseStatus, isAutoDirect = false) {
    let isSuccess = false;
    let fnGoPage;

    switch (responseStatus) {
      case mixinBackendErrorCode.success: {
        isSuccess = true;
        break;
      }
      case mixinBackendErrorCode.error: {
        isSuccess = false;
        break;
      }
      case mixinBackendErrorCode.authenticationError: {
        fnGoPage = this.mixinToLoginPage;
        isSuccess = false;
        break;
      }
      case mixinBackendErrorCode.noAuthorization: {
        fnGoPage = this.mixinToNoPermissionPage;
        isSuccess = false;
        break;
      }
      default: {
        isSuccess = false;
        break;
      }
    }

    if (isAutoDirect && fnGoPage) {
      fnGoPage();
    }

    return isSuccess;
  },

  // get cookie , if not exist return null
  mixinGetCookie(key) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${key}=`);
    if (parts.length === 2) {
      return parts
        .pop()
        .split(';')
        .shift();
    }
    return '';
  },
  mixinEraseCookie(key) {
    document.cookie = `${key}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`;
  },
  mixinGetCurrentLanguageCode() {
    const parsedUrl = new URL(window.location.href);
    let languageCode = parsedUrl.searchParams.get('Language');
    if (!languageCode) {
      languageCode = parsedUrl.pathname;
      if (languageCode) {
        const tempArray = languageCode.split('/');
        if (tempArray[2] === 'Language') {
          languageCode = tempArray[3].replace('-', '_');
        } else {
          // get
          languageCode = navigator.language;
          languageCode = languageCode.replace('-', '_');
        }
      }
    }

    return languageCode;
  },

  // useful api function
  async mixinLogoutProcess() {
    // call api
    const resObj = await this.mixinCallBackService(
      this.mixinBackendService.logout
    );

    if (resObj.status === mixinBackendErrorCode.success) {
      const languageCode = this.mixinGetCurrentLanguageCode();
      this.mixinToLoginPage(languageCode);
    }
  },
  async mixinAccountStatus() {
    // call api
    const resObj = await this.mixinCallBackService(
      this.mixinBackendService.checkStatus
    );

    return {
      isErrorAuth: resObj.status === mixinBackendErrorCode.authenticationError,
      isErrorNoAuth: resObj.status === mixinBackendErrorCode.noAuthorization,
      isSuccess: resObj.status === mixinBackendErrorCode.success,
    };
  },
  async mixinGetIpInfo() {
    return $.ajax({
      url: `https://api.ipify.org?format=json`,
      method: `GET`,
      dataType: `json`,
      success: resObject => {
        return resObject;
      },
      error: err => {
        console.error(err);
        return null;
      },
    });
  },

  // api list
  async mixinCallBackService(
    backendServiceName,
    payloadObject = null,
    isThrowException = false
  ) {
    // call api
    const _this = this;
    const backendUrl = `/${webpackDashboardName}/WebData/Server_Code/${backendServiceName}`;
    const errormessage = `ERROR API, ${backendServiceName}`;
    const fnResultHandler = (isSuccess, message, resultObj) => {
      // fail
      if (isSuccess === false && isThrowException) {
        console.error(message);
        return Promise.reject(new Error(message));
      }

      return resultObj;
    };

    let tempPayload;
    if (payloadObject !== null) {
      tempPayload = {
        ...payloadObject,
        UserId: payloadObject.UserId || this.mixinGetCookie(`UserID`),
      };
    }

    return $.ajax({
      url: backendUrl,
      method: `POST`,
      dataType: `json`,
      data: tempPayload,
      cache: false,
      success(resObject) {
        const isBNDGood = _this.mixinIsBackendStatusGood(
          resObject.status,
          isThrowException
        );
        return fnResultHandler(
          isBNDGood,
          isBNDGood ? null : errormessage,
          resObject
        );
      },
      error(result) {
        return fnResultHandler(false, errormessage);
      },
    });
  },
};
export default mixinFuncitons;
