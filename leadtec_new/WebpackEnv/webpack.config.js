const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const TerserPlugin = require('terser-webpack-plugin')
const VueLoaderPlugin = require('vue-loader/lib/plugin')
const isProductionMode = false;
const webpackDev = {
	host : "localhost",
	port : 8080
};
let connectionSetting = {
	webpackDashboardName: JSON.stringify("Leadtec")
};

let webpackSetting = {
	mode: 'development',
	entry: {
		"Main/LD_20210321_min_v1": "../WebData/JsFile/Internal/Main/SystemInitial.js",
	},
	devtool: 'cheap-source-map',//dev used
	devServer: {
		contentBase: './dist',
		hot: true,
		headers: {
			'Access-Control-Allow-Origin': '*'
		},
		port : webpackDev.port,
	},
	output: {
		filename: "[name].js",
		path: path.resolve(__dirname, '../WebData/BundleResult/'),
		publicPath: `http://${webpackDev.host}:${webpackDev.port}/JsFile/Internal/`,
		globalObject: 'this' //replace  global object with 'this', prevent error in webworker (window object is a global object ,but not exist in web worker )
	},
	module: {
		rules: [
			{
				test: /\.vue$/,
				loader: 'vue-loader'
			},
			{
				test: /\.js$/,
				exclude: /(node_modules|bower_components)/,
				use: {
					loader: 'babel-loader',
					options: {
						presets: [
							['babel-preset-env'].map(require.resolve),
							['babel-preset-stage-2'].map(require.resolve)
						],
					},
				},
			},
			{  //this will add jquery to window object
				test: require.resolve('./node_modules/jquery/dist/jquery.js'),
				use: [{
					loader: 'expose-loader',
					options: 'jQuery'
				}, {
					loader: 'expose-loader',
					options: '$'
				}]
			},
			{   // use plugin to compose all css file
				test: /\.css$/,
				oneOf: [
					// this matches `<style module>`
					{
						resourceQuery: /module/,
						use: [
							MiniCssExtractPlugin.loader,
							{
								loader: 'css-loader',
								options: {
									modules: true,
								}
							}
						]
					},
					// this matches plain `<style>` or `<style scoped>`
					{
						use: [
							MiniCssExtractPlugin.loader,
							'css-loader'
						]
					}
				]
			},
		],
	},
	resolve: {
		modules: [path.resolve(__dirname, './node_modules')],
		alias: {
			'vue': path.resolve(__dirname, './node_modules/vue/dist/vue.esm.js'),
			'jquery': path.resolve(__dirname, './node_modules/jquery/dist/jquery.js'),
		}
	},
	plugins: [
		new webpack.DefinePlugin(connectionSetting),
		new webpack.HotModuleReplacementPlugin(),
		new webpack.NamedModulesPlugin(),
		new webpack.ProvidePlugin({// only work in webpack module
			$: "jquery",
			jQuery: "jquery",
			'window.jQuery': 'jquery',
			'root.jQuery': 'jquery'
		}),
		new MiniCssExtractPlugin({
			filename: '[name]_CSS.css'
		}),
		new VueLoaderPlugin(),
	],
};

if (isProductionMode) {
	webpackSetting.mode = 'production';
	webpackSetting.devtool = 'none';
	let webpackDefinePlugin = new webpack.DefinePlugin({
		'process.env': {
			NODE_ENV: '"production"'
		}
	});

	let webpackCodeMinifyPlugin = new TerserPlugin({
		parallel: true,
		terserOptions: {
		ecma: 6,
		},
	});
	webpackSetting.plugins.unshift(webpackDefinePlugin, webpackCodeMinifyPlugin);
}

module.exports = webpackSetting;
