using csc8208Maui.Views.Verifier;
using csc8208Maui.Models;
using csc8208Maui.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
//using ZXing.Net.Mobile.Forms;
using Newtonsoft.Json;
//using Plugin.Toast;
using System.Diagnostics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Devices.Sensors;
using ZXing.Net.Maui.Readers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using csc8208Maui.ViewModels.Verifier;


namespace csc8208Maui.ViewModels
{
    partial class VerifierViewModel : ObservableObject
    {
        private EventStore todaysEvents = new EventStore();
        public ObservableCollection<Event> TodaysEventsList { get; set; }

        public Command SettingsCommand { get; }

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
                if (selectedEvent != null)
                {
                    ScanQRCode(selectedEvent);
                    SelectedEvent=null;
                } 
            }
        }

        [ObservableProperty]
        bool isDetecting=false;

        [ObservableProperty]
        bool scannerVisible=false;

        [ObservableProperty]
        int cameraViewHeight=0;

        [ObservableProperty]
        int listViewHeight=100;

        public VerifierViewModel()
        {
            SettingsCommand = new Command(OnSettingsButtonClicked);
            todaysEvents.GenerateFakeData();//Debug code
            //GenerateEvents();
            TodaysEventsList = new ObservableCollection<Event>(todaysEvents.GetItemsAsync(false).Result);
            //selectedEvent = TodaysEventsList.FirstOrDefault();
            HideCameraView();
        }

        private async void GenerateEvents()
        {
            EventStore downloadedEventStore = await WebService.GetEvents();
            if (downloadedEventStore != null)
            {
                todaysEvents = downloadedEventStore;
            }
            else
            {
                //CrossToastPopUp.Current.ShowToastWarning("Unable to retrieve data from server");
            }
        }

        

        private async void OnSettingsButtonClicked(object obj)
        {
            await Shell.Current.GoToAsync($"//verifier/{nameof(VerifierSettingsPage)}");
        }

        private async void ShowCameraView()
        {
            
            ScannerVisible=true;
            CameraViewHeight=100;
            ListViewHeight=0;
            IsDetecting=true;
            await Shell.Current.Navigation.PushModalAsync(new QRCodeScanner(new QRCodeScannerViewModel(selectedEvent)));
            
        }

        private async void HideCameraView()
        {
            ScannerVisible=false;
            CameraViewHeight=0;
            ListViewHeight=100;
            IsDetecting=false;
        }

        private async void ScanQRCode(Event eventToBeScanned)
        {
            
            Console.WriteLine($"Attempting to scan QR for event: {eventToBeScanned.ID}, {eventToBeScanned.Artist}, {eventToBeScanned.Location}");
            ShowCameraView();
            return;
        }
    }
}
