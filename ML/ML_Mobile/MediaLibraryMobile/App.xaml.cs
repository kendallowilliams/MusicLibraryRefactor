﻿using MediaLibraryMobile.Views;
using System;
using System.ComponentModel.Composition.Hosting;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.ComponentModel.Composition;
using System.ComponentModel;
using MediaLibraryMobile.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace MediaLibraryMobile
{
    [Export]
    public partial class App : Application, IDisposable
    {
        private readonly Lazy<MainController> lazyMainController;

        [ImportingConstructor]
        public App(Lazy<MainController> lazyMainController)
        {
            InitializeComponent();
            this.lazyMainController = lazyMainController;
            Device.SetFlags(new string[] { "MediaElement_Experimental" });
        }

        protected override void OnStart()
        {
            lazyMainController.Value.Startup();
            MainPage = lazyMainController.Value.GetMainView();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public void Dispose()
        {
            lazyMainController.Value.Shutdown();
        }
    }
}
