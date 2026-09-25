using csc8208Maui.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using csc8208Maui.Services;
using csc8208Maui.Models;
using System.Net.NetworkInformation;
using Newtonsoft.Json;
//using Plugin.Toast;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using csc8208Maui.Views.Verifier;
using csc8208Maui.Views.User;
using Android.Content.Res;
using csc8208Maui.Lib;

namespace csc8208Maui.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [ObservableProperty]
        private string username;
        
        [ObservableProperty]
        private Color usernameEntryColour;

        [ObservableProperty]
        private string password;
        
        [ObservableProperty]
        private Color passwordEntryColour;


        public Command LoginCommand { get; }
        public Command UserLoginCommand { get; }
        public Command RegisterCommand { get; }
        public Command DebugNavigateHomeCommand { get; }
        
        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            DebugNavigateHomeCommand = new Command(OnDebugNavigateHomeClicked);
            if (!WebService.CheckConnectionToInternet())
            {
                Console.WriteLine(WebService.connectionFailureMessage);
                //Needs replacing
                //CrossToastPopUp.Current.ShowToastWarning(WebService.connectionFailureMessage);
            }
            //Check for locally stored account info for offline use
            //string serialisedAccount = SecureStorage.GetAsync("account").Result;
            //WebService.account = JsonConvert.DeserializeObject<Account>(serialisedAccount);
            //string JWT = SecureStorage.GetAsync("JWT").Result;
            //WebService.SetHTTPHeaders(JWT);
            //WebService.GetAccountInfo();
            
            /* if(WebService.account is not null && JWT is not null)
            {
                if (WebService.account.verifier)
                {
                    Shell.Current.GoToAsync("//verifier");//Go to verifier landing page
                }
                else
                {
                    Shell.Current.GoToAsync("//user");//Go to user landing page
                }
            } */
            
            //===================================
            //DebugLogin();
            //===================================
        }
        
        private async void DebugLogin()
        {
            Username="joe";
            Password="password";
            OnLoginClicked(new object {});
        }

        private async void OnDebugNavigateHomeClicked(object obj)
        {
            
            //await Navigation.PushAsync(new RegistrationPage());
        }

        private async void OnLoginClicked(object obj)
        {
            (bool success, string message) loginResult = await WebService.LoginAsync(Username, Password);

            if (loginResult.success)
            {
                //WebService.account = WebService.GetAccountInfo();
                if (WebService.account.verifier)
                {
                    SwitchShell.GoToVerifier();
                    //await Shell.Current.GoToAsync($"//verifier/{nameof(VerifierLandingPage)}");//Go to verifier landing page
                }
                else
                {
                    SwitchShell.GoToUser();
                    //await Shell.Current.GoToAsync($"//user");//Go to user landing page
                }
                Username="";
                Password="";
            }
            else
            {
                if (loginResult.message.Equals(WebService.invalidCredentialsMessage))
                {
                    UsernameEntryColour = Colors.Red;
                    PasswordEntryColour = Colors.Red;
                    Console.WriteLine(WebService.invalidCredentialsMessage);
                    //CrossToastPopUp.Current.ShowToastError(WebService.invalidCredentialsMessage);
                }
                if (loginResult.message.Equals(WebService.connectionFailureMessage))
                {
                    Console.WriteLine(WebService.connectionFailureMessage);
                    //CrossToastPopUp.Current.ShowToastError(WebService.connectionFailureMessage);
                }
            }
        }

        
    }
}
