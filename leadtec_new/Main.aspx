<%@ Page Language="C#" AutoEventWireup="true" Inherits="Main" Codebehind="Main.aspx.cs" %>

<!DOCTYPE html>
<html>
    <head runat="server">
        <title>利慶工業 Leadtec</title>
        <meta http-equiv="Pragma" content="no-cache" />
        <meta http-equiv="Cache-Control" content="no-cache, must-revalidate"/>
        <meta http-equiv="expires" content="0" />
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
        <link rel="shortcut icon" href="/leadtec/WebData/Picture/icon/leadtec.ico">

        <!--import JS and CS file -->
<!--
        <script  type="text/javascript" src="/SIncomeStatement/WebData/BundleResult/Main/IS_20200516_min_v1.js"></script>
        <link rel="stylesheet" href="/IncomeStatement/WebData/BundleResult/Main/IS_20200516_min_v1_CSS.css" />
-->

        <script  type="text/javascript" src="http://localhost:8080/JsFile/Internal/Main/LD_20210321_min_v1.js"></script>
        <link rel="stylesheet" href="http://localhost:8080/JsFile/Internal/Main/LD_20210321_min_v1_CSS.css" />

    </head>
    <body>
        <div id="vue-instance">
        <!-- Header -->
            <header id="header">
                <div class="inner">
                    <nav id="nav">
                        <a v-for="mm in menuAry" href="index.html">
                            {{languageState[mm.key]}}
                        </a>
                    </nav>
                    <a href="#navPanel" class="navPanelToggle"><span class="fa fa-bars"></span></a>
                </div>
            </header>

        <!-- Banner -->
            <section id="banner">
                <div class="inner">
                    <header>
                        <h1>{{languageState["leadtec"]}}</h1>
                    </header>

                    <div class="flex">
                        <div v-for="feature in featureAry">
                            <component v-bind:is="feature.icon" :size="48" fillColor="#FF0000"></component>
                            <h3>{{languageState[feature.title]}}</h3>
                            <p>{{languageState[feature.detail]}}</p>
                        </div>
                    </div>

                    <footer>
                        <a href="javascript:;" class="button">{{languageState["SeeMore"]}}</a>
                    </footer>
                </div>
            </section>


        <!-- Three -->
            <section id="three" class="wrapper align-center">
                <div class="inner">
                    <div class="flex flex-2">
                        <article v-for="intro in introMainAry">
                            <header>
                                <h3>{{languageState[intro.title]}}</h3>
                            </header>
                            <div
                                v-for="picSrc in intro.picAry"
                                :style="{
                                    backgroundImage: 'url(' + picSrc + ')',
                                    width: '160px',
                                    height: '160px',
                                    backgroundSize: 'cover',
                                    display: 'inline-block',
                                    margin:'0.4em',
                                    borderRadius: '8px'}"></div>
                        </article>
                    </div>
                </div>
            </section>

        <!-- Four -->
            <section id="four" class="wrapper" style="background: url('http://www.leadtec.com.tw/upload/Image/main/1108_4.jpg');background-size: cover;background-repeat: no-repeat;">
                <div class="inner">
                    <div class="flex flex-2" style="color: white;width: 35%">
                        <h3 style="color: white">{{languageState['company_philosophy']}}</h3>
                        <p>{{languageState['company_intro_1']}}</p>
                        <p>{{languageState['company_intro_2']}}<p>
                    </div>
                </div>
            </section>
        <!-- Five -->
            <section id="five" class="wrapper align-center" style="padding: 6em 0 2em 0;">
                <iframe
                    frameborder="0"
                    width="90%"
                    height="500"
                    marginheight="0"
                    marginwidth="0"
                    scrolling="no"
                    src="https://maps.google.com.tw/maps?f=q&amp;source=s_q&amp;hl=zh-TW&amp;geocode=&amp;q=%E5%BD%B0%E5%8C%96%E7%B8%A3%E7%A6%8F%E8%88%88%E9%84%89%E7%A6%8F%E8%88%88%E5%B7%A5%E6%A5%AD%E5%8D%80%E7%A6%8F%E5%B7%A5%E8%B7%AF25%E8%99%9F&amp;aq=&amp;sll=24.012601,120.493616&amp;sspn=0.001335,0.001725&amp;brcurrent=3,0x346947e0e9af21fd:0x2fbfa2a1bff03550,0,0x3469491eb5791475:0xd6e84b58ba347f27&amp;ie=UTF8&amp;hq=&amp;hnear=506%E5%BD%B0%E5%8C%96%E7%B8%A3%E7%A6%8F%E8%88%88%E9%84%89%E7%A6%8F%E5%B7%A5%E8%B7%AF25%E8%99%9F&amp;t=m&amp;ll=24.008522,120.490236&amp;spn=0.037634,0.054846&amp;z=14&amp;iwloc=A&amp;output=embed"
                ></iframe>
            </section>

            <footer class="align-center" style="background: #DD0012;">
                <p style="margin: 0;color: white">LEADTEC CO., LTD. 利慶工業股份有限公司</p>
                <p style="margin: 0;color: white">&copy; 2021 LEADTEC</p>
            </footer>
        </div>
    </body>
</html>

<style scoped>
#leftMenu {
    position: relative;
    width: 16%;
    background-color: #61C7D0;
    color: #000;
    overflow-y: auto;
    border-radius: 1vw;
    margin:32px 0 16px 16px;
    display: inline-block;
    float: left;
}
#title {
    font-size: 14px;
    text-align: center;
    padding: 11px 8px 0 8px;
}
.menuItem {
    font-size: 0.9rem;
    width: 100%;
    text-align: center;
    padding: 8px;
    background-color: #138496;
}
.menuItem:hover {
    background-color: #93D2D8;
}
.dropIcon {
    position: absolute;
    right: 8px;
}
.submenuUL {
    font-size: 17px;
    text-align: left;
    padding-left: 8%;
    list-style: none;
}
.submenuUL li {
    padding: 8px;
}
.submenuUL li:hover{
    background-color: #93D2D8;
}
#btnLogout {
    bottom: 8%;
    padding: 8px;
    background-color: #6E7E85;
    text-align: center;
    width: 100%;
}
#btnLogout:hover {
    background-color: #6EA7AC;
}
#rightContent {
    position: relative;
    display: inline-block;
    float: left;
    width: 80%;
    min-height: 500px;
    border-radius: 1vw;
    margin: 16px 0 16px 16px;
}
#helloMessage {
    text-align: center;
    margin-top: 60px;
    color: #137C8D;
}
</style>
