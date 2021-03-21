<%@ Page Language="C#" AutoEventWireup="true" Inherits="Main" Codebehind="Main.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Leadtec</title>
        <meta http-equiv="Pragma" content="no-cache" />
        <meta http-equiv="Cache-Control" content="no-cache, must-revalidate"/>
        <meta http-equiv="expires" content="0" />
        <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />

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
                        <a v-for="mm in menu" href="index.html">
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
                        <h1>Welcome to Projection</h1>
                    </header>

                    <div class="flex ">

                        <div>
                            <span class="icon fa-car"></span>
                            <h3>Aliquam</h3>
                            <p>Suspendisse amet ullamco</p>
                        </div>

                        <div>
                            <span class="icon fa-camera"></span>
                            <h3>Elementum</h3>
                            <p>Class aptent taciti ad litora</p>
                        </div>

                        <div>
                            <span class="icon fa-bug"></span>
                            <h3>Ultrices</h3>
                            <p>Nulla vitae mauris non felis</p>
                        </div>

                    </div>

                    <footer>
                        <a href="#" class="button">Get Started</a>
                    </footer>
                </div>
            </section>


        <!-- Three -->
            <section id="three" class="wrapper align-center">
                <div class="inner">
                    <div class="flex flex-2">
                        <article>
                            <div class="image round">
                                <img src="images/pic01.jpg" alt="Pic 01" />
                            </div>
                            <header>
                                <h3>Lorem ipsum<br /> dolor amet nullam</h3>
                            </header>
                            <p>Morbi in sem quis dui placerat ornare. Pellentesquenisi<br />euismod in, pharetra a, ultricies in diam sed arcu. Cras<br />consequat  egestas augue vulputate.</p>
                            <footer>
                                <a href="#" class="button">Learn More</a>
                            </footer>
                        </article>
                        <article>
                            <div class="image round">
                                <img src="images/pic02.jpg" alt="Pic 02" />
                            </div>
                            <header>
                                <h3>Sed feugiat<br /> tempus adipicsing</h3>
                            </header>
                            <p>Pellentesque fermentum dolor. Aliquam quam lectus<br />facilisis auctor, ultrices ut, elementum vulputate, nunc<br /> blandit ellenste egestagus commodo.</p>
                            <footer>
                                <a href="#" class="button">Learn More</a>
                            </footer>
                        </article>
                    </div>
                </div>
            </section>

        <!-- Footer -->
            <footer id="footer">
                <div class="inner">

                    <h3>Get in touch</h3>

                    <form action="#" method="post">

                        <div class="field half first">
                            <label for="name">Name</label>
                            <input name="name" id="name" type="text" placeholder="Name">
                        </div>
                        <div class="field half">
                            <label for="email">Email</label>
                            <input name="email" id="email" type="email" placeholder="Email">
                        </div>
                        <div class="field">
                            <label for="message">Message</label>
                            <textarea name="message" id="message" rows="6" placeholder="Message"></textarea>
                        </div>
                        <ul class="actions">
                            <li><input value="Send Message" class="button alt" type="submit"></li>
                        </ul>
                    </form>

                    <div class="copyright">
                        &copy; Untitled. Design: <a href="https://templated.co">TEMPLATED</a>. Images: <a href="https://unsplash.com">Unsplash</a>.
                    </div>

                </div>
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
