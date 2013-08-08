/*
 Licensed to the Apache Software Foundation (ASF) under one
 or more contributor license agreements.  See the NOTICE file
 distributed with this work for additional information
 regarding copyright ownership.  The ASF licenses this file
 to you under the Apache License, Version 2.0 (the
 "License"); you may not use this file except in compliance
 with the License.  You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing,
 software distributed under the License is distributed on an
 "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 KIND, either express or implied.  See the License for the
 specific language governing permissions and limitations
 under the License. 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using WPCordovaClassLib;
using System.Xml.Linq;


namespace ApplicationTemplate {
    public partial class MainPage : PhoneApplicationPage {

        private SupportedPageOrientation supportedPageOrientation = SupportedPageOrientation.PortraitOrLandscape;
        private bool showSplashScreen = true;

        // Constructor
        public MainPage() {
            InitializeComponent();
            LoadPreferences();
            this.CordovaView.Loaded += CordovaView_Loaded;
            WebBrowser browser = ((Grid)(this.CordovaView.Content)).Children[0] as WebBrowser;
            browser.Navigated += browser_Navigated;
            browser.ScriptNotify += browser_ScriptNotify;
        }

        void browser_ScriptNotify(object sender, NotifyEventArgs e) {
            if(e.Value == "DevExpress.ExitApp") {
                Application.Current.Terminate();
            }
        }

        void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            WebBrowser browser = sender as WebBrowser;
            browser.InvokeScript("eval", "var DevExpressLocalUrlPrefix = \"x-wmapp0:www/\";");
        }

        private void LoadPreferences() {

            StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri("config.xml", UriKind.Relative));

            if(streamInfo != null) {
                StreamReader sr = new StreamReader(streamInfo.Stream);

                //This will Read Keys Collection for the xml file
                XDocument document = XDocument.Parse(sr.ReadToEnd());

                var preferences = from results in document.Descendants("preference")
                                  select new {
                                      name = (string)results.Attribute("name"),
                                      value = (string)results.Attribute("value")
                                  };

                foreach(var pref in preferences) {
                    if(pref.name == "orientation") {
                        if(pref.value == "portrait") {
                            supportedPageOrientation = SupportedPageOrientation.Portrait;
                        }
                        if(pref.value == "landscape") {
                            supportedPageOrientation = SupportedPageOrientation.Landscape;
                        }
                    }

                    if(pref.name == "showsplashscreen") {
                        if(pref.value == "false") {
                            showSplashScreen = false;
                        }
                    }
                }
            }
        }

        private void CordovaView_Loaded(object sender, RoutedEventArgs e) {
            this.CordovaView.Loaded -= CordovaView_Loaded;
            // first time load will have an animation
            if(showSplashScreen) {
                Storyboard _storyBoard = new Storyboard();
                DoubleAnimation animation = new DoubleAnimation() {
                    From = 0,
                    Duration = TimeSpan.FromSeconds(0.6),
                    To = 90
                };
                Storyboard.SetTarget(animation, SplashProjector);
                Storyboard.SetTargetProperty(animation, new PropertyPath("RotationY"));
                _storyBoard.Children.Add(animation);
                _storyBoard.Begin();
                _storyBoard.Completed += Splash_Completed;
            }
            else {
                LayoutRoot.Children.Remove(SplashImage);
                this.SupportedOrientations = supportedPageOrientation;
            }
        }

        void Splash_Completed(object sender, EventArgs e) {
            (sender as Storyboard).Completed -= Splash_Completed;
            LayoutRoot.Children.Remove(SplashImage);
            this.SupportedOrientations = supportedPageOrientation;
        }
    }
}
