using csc8208Maui.Views.Verifier;
using csc8208Maui.Models;
using csc8208Maui.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace csc8208Maui.ViewModels.Verifier
{
    class QRCodeDecisionViewModel : BaseViewModel
    {
        //Details of the event that the verifier is guarding, not the contents of the QR code.
        private Event selectedEvent;
        public Event SelectedEvent
        {
            get
            {
                return selectedEvent;
            }
            set
            {
                selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }

        private int decision;
        public int Decision
        {
            get
            {
                return decision;
            }
            set
            {
                decision = value;
                OnPropertyChanged(nameof(Decision));
            }
        }

        private string decisionDetails;
        public string DecisionDetails
        {
            get
            {
                return decisionDetails;
            }
            set
            {
                decisionDetails = value;
                OnPropertyChanged(nameof(DecisionDetails));
            }
        }


        public QRCodeDecisionViewModel(Event selectedEvent, int decision, string decisionDetails)
        {
            this.selectedEvent = selectedEvent;
            this.decision = decision;
            this.decisionDetails = decisionDetails;
        }
    }
}
