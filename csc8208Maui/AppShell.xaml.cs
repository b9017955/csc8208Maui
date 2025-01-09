using csc8208Maui.ViewModels;
using csc8208Maui.Views;
using csc8208Maui.Views.User;
using csc8208Maui.Views.Verifier;
using System;
using System.Collections.Generic;

namespace csc8208Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute($"user/{nameof(UserSettingsPage)}",typeof(UserSettingsPage));
            Routing.RegisterRoute($"verifier/{nameof(VerifierSettingsPage)}", typeof(VerifierSettingsPage));
            Routing.RegisterRoute($"login/{nameof(RegistrationPage)}", typeof(RegistrationPage));
        }
    }
}
