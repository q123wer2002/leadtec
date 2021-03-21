/* eslint-disable no-undef */
// for IE 11 used
import 'babel-polyfill';
import 'url-polyfill';
// for IE 11 used  End

import './Vue/Instance/IVue_Login.js';
import '../../../CSS/External/googlefont.css';
import '../../../CSS/Internal/Login/Signin.css';

$(window).on('load', () => {
  IncomeStatement.js_Vue_Instance.IVue_Login.Intital();
});
// develop code for webpakc HMR Use

if (module.hot) {
  module.hot.accept();
}
/* eslint-disable no-undef */
