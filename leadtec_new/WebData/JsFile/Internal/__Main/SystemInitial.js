/* eslint-disable no-undef */
// for IE 11 used
import 'babel-polyfill';
import 'url-polyfill';

// import js file
import './Vue/Instance/IVue_Initial.js';

// import css
import '../../../CSS/Internal/Main/layout.css';

$(window).on('load', () => {
  // start
  IncomeStatement.js_Vue_Instance.IVue_Initial.Intital();
});

// develop code for webpakc HMR Use
if (module.hot) {
  module.hot.accept();
}
/* eslint-disable no-undef */
