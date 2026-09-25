using System;
using csc8208Maui.Views;
using csc8208Maui.Views.User;
using csc8208Maui.Views.Verifier;

namespace csc8208Maui.Lib;

//App contains 2 shells and 1 navigation page (login)
public static class SwitchShell
{
    public static void GoToLogin()
    {
        var loginPage = Application.Current.Handler.MauiContext.Services.GetService<LoginPage>();
        Application.Current.MainPage = new NavigationPage(loginPage);
    }

    public static void GoToVerifier()
    {
        var VerifierShell = Application.Current.Handler.MauiContext.Services.GetService<VerifierLandingPage>();
        Application.Current.MainPage = VerifierShell;
    }

    public static void GoToUser()
    {
        var UserShell = Application.Current.Handler.MauiContext.Services.GetService<UserLandingPage>();
        Application.Current.MainPage = UserShell;
    }
}
