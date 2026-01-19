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

namespace csc8208Maui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
                UsernameEntryColour = null;
            }
        }

        private Color usernameEntryColour;
        public Color UsernameEntryColour 
        { 
            get 
            { 
                return usernameEntryColour; 
            }
            set
            {
                usernameEntryColour = value;
                OnPropertyChanged(nameof(UsernameEntryColour));
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
                PasswordEntryColour = null;
            }
        }
        private Color passwordEntryColour;
        public Color PasswordEntryColour
        {
            get
            {
                return passwordEntryColour;
            }
            set
            {
                passwordEntryColour = value;
                OnPropertyChanged(nameof(PasswordEntryColour));
            }
        }


        public Command LoginCommand { get; }
        public Command UserLoginCommand { get; }
        public Command RegisterCommand { get; }
        public Command DebugNavigateHomeCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            RegisterCommand = new Command(OnRegisterClicked);
            DebugNavigateHomeCommand = new Command(OnDebugNavigateHomeClicked);

            //Check for locally stored account info for offline use
            string serialisedAccount = SecureStorage.GetAsync("account").Result;

            string JWT = SecureStorage.GetAsync("JWT").Result;
            if (WebService.account != null && JWT != null)
            {
                WebService.account = JsonConvert.DeserializeObject<Account>(serialisedAccount);
                WebService.SetHTTPHeaders(JWT);
                if (!WebService.CheckConnectionToInternet())
                {
                    Console.WriteLine(WebService.connectionFailureMessage);
                    //Needs replacing
                    //CrossToastPopUp.Current.ShowToastWarning(WebService.connectionFailureMessage);
                }
                if (WebService.account.verifier)
                {
                    Shell.Current.GoToAsync("//verifier");//Go to verifier landing page
                }
                else
                {
                    Shell.Current.GoToAsync("//user");//Go to user landing page
                }
            }


        }
        
        private async void OnDebugNavigateHomeClicked(object obj)
        {
            await Shell.Current.GoToAsync("//user");
        }

        private async void OnLoginClicked(object obj)
        {
            //WebService.InitialiseNewAppSignature();
            //Debug code==========
            /* Console.WriteLine($"Username: {username}, Password: {password}");
            if (username!=null && username.Equals("user"))
            {
                SecureStorage.Remove("tickets");
                //SecureStorage.SetAsync("DEBUGPUBLICKEY", SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);//for debugging purposes only
                Console.WriteLine(SecureStorage.GetAsync("DEBUGPUBLICKEY").Result);
                await Shell.Current.GoToAsync("//user");
            }
            else
            {
                await Shell.Current.GoToAsync("//verifier");
            }
            
            return; */
            //====================

            (bool success, string message) loginResult = await WebService.LoginAsync(username, password);

            if (loginResult.success)
            {
                //WebService.account = WebService.GetAccountInfo();
                if (WebService.account.verifier)
                {
                    await Shell.Current.GoToAsync("//verifier");//Go to verifier landing page
                }
                else
                {
                    (bool success, string message) updatePublicKeyResult = WebService.UpdatePublicKey();
                    if(updatePublicKeyResult.success!=null && updatePublicKeyResult.message != null)
                    {
                        if (updatePublicKeyResult.success)
                        {
                            Console.WriteLine("updated the public key");
                            //CrossToastPopUp.Current.ShowToastSuccess("Updated public key");
                        }
                        else
                        {
                            Console.WriteLine(updatePublicKeyResult.message);
                            //CrossToastPopUp.Current.ShowToastError(updatePublicKeyResult.message);
                        }
                    }
                    await Shell.Current.GoToAsync("//user");//Go to user landing page
                }
            }
            else
            {
                if (loginResult.message.Equals(WebService.invalidCredentialsMessage))
                {
                    usernameEntryColour = Colors.Red;
                    passwordEntryColour = Colors.Red;
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

        private async void OnRegisterClicked(object obj)
        {
            await Shell.Current.GoToAsync($"login/{nameof(RegistrationPage)}");
        }
    }
}
