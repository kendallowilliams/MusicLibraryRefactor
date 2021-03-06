﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using MediaLibraryMobile.Controllers;
using MediaLibraryMobile.Services;
using MediaLibraryMobile.Services.Interfaces;
using MediaLibraryMobile.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using Xamarin.Essentials;
using XPlatform = Xamarin.Essentials.Platform;

namespace MediaLibraryMobile.Droid
{
    [Activity(MainLauncher = true)]
    public class LoginActivity : Activity
    {
        private LoginController loginController;
        private LoginViewModel loginViewModel;
        private Login binding;

        [Import]
        private ILogService logService;

        public LoginActivity()
        {
            using var container = MefService.GetMEFContainer(new List<Assembly>() { typeof(MainActivity).Assembly }, this);
            loginController = container.GetExportedValue<LoginController>();
            loginViewModel = loginController.LoginViewModel;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            string here = $"{nameof(LoginActivity)}->{nameof(OnCreate)}";

            base.OnCreate(savedInstanceState);
            XPlatform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.Login);

            loginController.Startup();
            binding = new Login(this);
            binding.btnLogin.Click += LoginClicked;
            binding.txtUsername.TextChanged += (sender, args) => loginViewModel.Username = String.Concat(args.Text);
            binding.txtPassword.TextChanged += (sender, args) => loginViewModel.Password = String.Concat(args.Text);
            binding.chkRememberMe.CheckedChange += (sender, args) => loginViewModel.RememberMe = args.IsChecked;
            loginViewModel.RememberMe = Preferences.Get(nameof(LoginViewModel.RememberMe), default(bool), "login");

            logService.Info(here, $"{nameof(LoginViewModel.RememberMe)}: {loginViewModel.RememberMe}");
            if (loginViewModel.RememberMe) /*then*/ binding.btnLogin.CallOnClick();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            XPlatform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void LoginClicked(object sender, EventArgs args)
        {
            binding.loginLayout.Visibility = ViewStates.Gone;
            binding.progressBarLayout.Visibility = ViewStates.Visible;
            await loginController.Login(LoginSucceeded, LoginFailed);
        }

        private void LoginSucceeded()
        {
            binding.lblError.Text = string.Empty;
            RunOnUiThread(() => StartActivity(typeof(MainActivity)));
            Finish();
        }

        private void LoginFailed()
        {
            binding.lblError.Text = "Invalid username/password.";
            binding.loginLayout.Visibility = ViewStates.Visible;
            binding.progressBarLayout.Visibility = ViewStates.Gone;
        }
    }
}