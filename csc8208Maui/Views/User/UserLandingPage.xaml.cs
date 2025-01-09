using csc8208Maui.ViewModels;
using csc8208Maui.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace csc8208Maui.Views.User
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserLandingPage : Shell
    {
        public UserLandingPage()
        {
            InitializeComponent();
            BindingContext = new UserViewModel();
            //CurrentPageChanged += OnCurrentPageChanged;
        }

        private void OnCurrentPageChanged(object sender, EventArgs e)
        {
            
            /*if (CurrentPage.TabIndex==2)
            {
                UserAccountViewModel userAccountViewModel = (UserAccountViewModel)CurrentPage.BindingContext;
                userAccountViewModel.UpdateTickets("OUTSIDE_CALL_TAB");
            }*/
        }

        
    }
}