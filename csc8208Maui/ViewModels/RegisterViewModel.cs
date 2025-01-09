using csc8208Maui.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
//using Plugin.Toast;
using csc8208Maui.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace csc8208Maui.ViewModels
{
    public class RegisterViewModel:BaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isVerifier = false;
        public bool IsVerifier 
        { 
            get 
            { 
                return isVerifier; 
            } 
            set
            {
                isVerifier = value;
                OnPropertyChanged(nameof(IsVerifier));
            }
        }

        private string emailAddress;
        public string EmailAddress
        {
            get
            {
                return emailAddress;
            }
            set
            {
                emailAddress = value;
                OnPropertyChanged(nameof(EmailAddress));
                EmailEntryColour = null;
            }
        }

        private Color emailEntryColour;
        public Color EmailEntryColour
        {
            get
            {
                return emailEntryColour;
            }
            set
            {
                emailEntryColour = value;
                OnPropertyChanged(nameof(EmailEntryColour));
            }
        }

        private string firstName;
        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
                OnPropertyChanged(nameof(FirstName));
                FirstNameEntryColour = null;
            }
        }

        private Color firstNameEntryColour;
        public Color FirstNameEntryColour
        {
            get
            {
                return firstNameEntryColour;
            }
            set
            {
                firstNameEntryColour = value;
                OnPropertyChanged(nameof(FirstNameEntryColour));
            }
        }

        private string secondName;
        public string SecondName
        {
            get
            {
                return secondName;
            }
            set
            {
                secondName = value;
                OnPropertyChanged(nameof(SecondName));
                SecondNameEntryColour = null;
            }
        }

        private Color secondNameEntryColour;
        public Color SecondNameEntryColour
        {
            get
            {
                return secondNameEntryColour;
            }
            set
            {
                secondNameEntryColour = value;
                OnPropertyChanged(nameof(SecondNameEntryColour));
            }
        }

        private string password;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
                PasswordEntryColour = null;
            }
        }

        private string confirmPassword;
        public string ConfirmPassword
        {
            get
            {
                return confirmPassword;
            }
            set
            {
                confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
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

        private bool signUpButtonClickable=true;
        public bool SignUpButtonClickable
        {
            get
            {
                return signUpButtonClickable;
            }
            set
            {
                signUpButtonClickable = value;
                OnPropertyChanged(nameof(SignUpButtonClickable));
            }
        }

        public Command SignUpCommand { get; }

        public RegisterViewModel()
        {
            SignUpCommand = new Command(OnSignUpClicked);
        }

        private async void OnSignUpClicked(object obj)
        {
            Console.WriteLine("SIGN UP CLICKED");
            bool problems = false;
            EmailEntryColour = emailAddress == null ? Colors.Red : null;
            PasswordEntryColour = password == null || confirmPassword == null ? Colors.Red : null;
            FirstNameEntryColour = firstName == null ? Colors.Red : null;
            SecondNameEntryColour = secondName == null ? Colors.Red : null;

            if (emailAddress != null && password != null && confirmPassword != null && firstName != null && secondName != null)
            {
                if (!password.Equals(confirmPassword))
                {
                    PasswordEntryColour = Colors.Red;
                    problems = true;
                    Console.WriteLine("PASSWORD MISMATCH");
                }
                Regex rx = new Regex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                if (!rx.IsMatch(emailAddress))
                {
                    EmailEntryColour = Colors.Red;
                    problems = true;
                    Console.WriteLine("INVALID USERNAME");
                }
                if (firstName.Length < 1)
                {
                    FirstNameEntryColour = Colors.Red;
                    problems = true;
                    Console.WriteLine("Name must contain more than 1 character");
                }
                if (secondName.Length < 1)
                {
                    SecondNameEntryColour = Colors.Red;
                    problems = true;
                    Console.WriteLine("Name must contain more than 1 character");
                }
                if (!problems) 
                {
                    //Submit form to server
                    //If this were a real app we would send a verification email but that is unnecessary for a proof of concept.
                    (bool success, string message) registrationResult = WebService.Register(emailAddress, password, firstName, secondName, isVerifier);
                    if (registrationResult.success)
                    {
                        await Shell.Current.GoToAsync("//login");
                    }
                    else
                    {
                        Console.WriteLine(registrationResult.message);
                    }
                    
                }
            }
            else
            {
                Console.WriteLine("Please fill in the form");
            }
        }
    }
}
