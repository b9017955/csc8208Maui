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

namespace csc8208Maui.ViewModels
{
    class VerifierViewModel: BaseViewModel
    {
        private EventStore todaysEvents = new EventStore();
        public ObservableCollection<Event> TodaysEventsList { get; set; }

        public Command LogoutCommand { get; }
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
                if(selectedEvent!=null) ScanQRCode(selectedEvent);
            }
        }

        public VerifierViewModel()
        {
            LogoutCommand = new Command(SignOutButtonClicked);
            SettingsCommand = new Command(OnSettingsButtonClicked);
            
            todaysEvents.GenerateFakeData();//Debug code
            //GenerateEvents();
            TodaysEventsList = new ObservableCollection<Event>(todaysEvents.GetItemsAsync(false).Result);
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

        private async void SignOutButtonClicked(object obj)
        {
            if (WebService.Logout())
            {
                //Logout successful
                await Shell.Current.GoToAsync($"//login");
            }
            else
            {
                Console.WriteLine(WebService.logoutErrorMessage);
            }
        }

        private async void OnSettingsButtonClicked(object obj)
        {
            await Shell.Current.GoToAsync($"verifier/{nameof(VerifierSettingsPage)}");
        }

        private async void ScanQRCode(Event eventToBeScanned)
        {
            Console.WriteLine($"Attempting to scan QR for event: {eventToBeScanned.ID}, {eventToBeScanned.Artist}, {eventToBeScanned.Location}");
            return;
            //ZXing package unsupported; rewrite.
            /*var scan = new ZXingScannerPage();
            await Shell.Current.Navigation.PushModalAsync(scan);
            scan.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.Navigation.PopModalAsync();
                    Console.WriteLine($"QRCODE OUTPUT: {result.Text}");
                    Console.WriteLine("Starting Timer...");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    //Extract data from QR Code
                    //WebService.InitialiseNewAppSignature();
                    string scannedEncodedQRCodeData = result.Text;
                    QRCode scannedQRCodeData;
                    try
                    {
                        scannedQRCodeData = JsonConvert.DeserializeObject<QRCode>(scannedEncodedQRCodeData);
                    }
                    catch
                    {
                        await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "ERROR SCANNING QR CODE"));
                        SelectedEvent = null;
                        return;
                    }
                    (Account accountInfo, Ticket ticketInfo) ticketInfoFromServer = WebService.VerifyTicket(scannedQRCodeData.serverSignedTicket).Result;
                    (bool timeStampDecision, string timeStampDecisionDetails) timeStampVerificationOutcome = WebService.VerifyTimeStamp(scannedQRCodeData.appSignedTimeStamp, ticketInfoFromServer.accountInfo.appPublicKey);
                    //(bool timeStampDecision, string timeStampDecisionDetails) timeStampVerificationOutcome = WebService.VerifyTimeStamp(scannedQRCodeData.appSignedTimeStamp, SecureStorage.GetAsync("DEBUGPUBLICKEY").Result);
                    Console.WriteLine($"£ {timeStampVerificationOutcome.timeStampDecisionDetails}");
                    
                    int overallDecision;//0=Denied, 1=Approved, 2=Further Action Required
                    string overallDecisionDetails;
                    
                    if (ticketInfoFromServer.accountInfo != null && ticketInfoFromServer.ticketInfo != null)
                    {
                        if (ticketInfoFromServer.ticketInfo.Artist.Equals(eventToBeScanned.Artist) &&
                        ticketInfoFromServer.ticketInfo.EventLocation.Equals(eventToBeScanned.Location) &&
                        ticketInfoFromServer.ticketInfo.DoorsOpen.Equals(eventToBeScanned.DoorsOpen))
                        {
                            overallDecision = timeStampVerificationOutcome.timeStampDecision ? 1 : 2;
                            overallDecisionDetails = $"TICKET IS VALID::{timeStampVerificationOutcome.timeStampDecisionDetails}::FirstName:{ticketInfoFromServer.accountInfo.firstName}, SecondName:{ticketInfoFromServer.accountInfo.secondName}";
                        }
                        else
                        {
                            overallDecision = 0;
                            overallDecisionDetails = $"TICKET IS NOT VALID FOR THIS EVENT::{timeStampVerificationOutcome.timeStampDecisionDetails}";
                        }
                    }
                    else
                    {
                        overallDecision = 0;
                        overallDecisionDetails = $"TICKET IS INVALID";
                    }
                    await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, overallDecision, overallDecisionDetails));
                    stopwatch.Stop();
                    Console.WriteLine($"Time taken to verify QR Code: {stopwatch.Elapsed.TotalMilliseconds} ms");
                    SelectedEvent = null;
                });
            };*/
        }
    }
}
