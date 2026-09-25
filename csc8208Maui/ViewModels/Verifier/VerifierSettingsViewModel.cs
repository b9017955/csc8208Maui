using csc8208Maui.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using csc8208Maui.Services;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration;
using csc8208Maui.Lib;

namespace csc8208Maui.ViewModels
{
    class VerifierSettingsViewModel
    {
        public Command LogoutCommand { get; }
        public VerifierSettingsViewModel()
        {
            LogoutCommand = new Command(OnSignOutButtonClicked);
        }
        private async void OnSignOutButtonClicked(object obj)
        {
            //TODO: Code for signing out.
            WebService.Logout();
            //await Shell.Current.DisplayAlertAsync("test","test message", "alright");
            //await Shell.Current.GoToAsync("//verifier");
            //await Shell.Current.GoToAsync("//login/LoginPage");
            // Get the current navigation stack within the active Tab
            
            //await Shell.Current.GoToAsync("//login");
            SwitchShell.GoToLogin();
        }
    }
}
