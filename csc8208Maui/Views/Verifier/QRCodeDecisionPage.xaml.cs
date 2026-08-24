using csc8208Maui.Models;
using csc8208Maui.ViewModels.Verifier;
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
    public partial class QRCodeDecisionPage : ContentPage
    {
        //decision 0 denied, decision 1 approved, decision 2 further action required
        public QRCodeDecisionPage(Event eventBeingScanned, int decision, string decisionDetails)
        {
            InitializeComponent();
            this.BindingContext = new QRCodeDecisionViewModel(eventBeingScanned, decision, decisionDetails);
        }
    }
}