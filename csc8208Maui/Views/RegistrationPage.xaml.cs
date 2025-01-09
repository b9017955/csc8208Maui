using csc8208Maui.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace csc8208Maui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
            RegisterViewModel viewModel = new RegisterViewModel();
            this.BindingContext = viewModel;
        }

        //When the user taps the password or confirm password field the background colour should turn to the default colour if it has been turned red from a  password mismatch
        void OnPasswordFieldTap(object sender, EventArgs e)
        {
            
        }
    }
}