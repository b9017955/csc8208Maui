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

namespace csc8208Maui.Views.Verifier
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VerifierSettingsPage : ContentPage
    {
        public VerifierSettingsPage()
        {
            InitializeComponent();
            this.BindingContext = new VerifierSettingsViewModel();
        }
    }
}