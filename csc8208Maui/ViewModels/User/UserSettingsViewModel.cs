using csc8208Maui.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace csc8208Maui.ViewModels
{
    class UserSettingsViewModel:BaseViewModel
    {
        public Command LogoutCommand { get; }
        public UserSettingsViewModel()
        {
            LogoutCommand = new Command(OnSignOutButtonClicked);
        }
        private async void OnSignOutButtonClicked(object obj)
        {
            //TODO: Code for signing out.
            await Shell.Current.GoToAsync($"//login");
        }
    }
}
