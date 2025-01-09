using csc8208Maui.Models;
using csc8208Maui.Services;
using csc8208Maui.Views.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Timers;
//using ZXing.Common;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace csc8208Maui.ViewModels.User
{
    class UserAccountViewModel : BaseViewModel
    {
        private UserTickets userTickets = new UserTickets();
        private ObservableCollection<Ticket> tickets = new ObservableCollection<Ticket>();
        public ObservableCollection<Ticket> Tickets 
        { 
            get 
            {
                return tickets;    
            } 
            set 
            {
                tickets = value; 
                OnPropertyChanged(nameof(Tickets)); 
            } 
        }
        public Command LogoutCommand { get; }
        public Command SettingsCommand { get; }
        //public EncodingOptions BarcodeOptions => new EncodingOptions() { Height = 300, Width = 300, PureBarcode = true };
        private System.Timers.Timer timer;
        

        public UserAccountViewModel()
        {
            LogoutCommand = new Command(OnSignOutButtonClicked);
            SettingsCommand = new Command(OnSettingsButtonClicked);
            
            SecureStorage.SetAsync("DEBUGPUBLICKEY", SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);//for debugging purposes only
            UpdateTickets("INITIAL_CALL");

            //DebugGenerateQRCode();

            StartTimer();
        }

        private void StartTimer()
        {
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => RegenerateQRCodes("AUTOREGEN"));
        }

        public async void UpdateTickets(string msg)
        {
            // When the tickets were purchased they should have been stored to local storage, so first check SecureStorage
            string serialisedTickets = SecureStorage.GetAsync("tickets").Result;
            Console.WriteLine($"{msg}:^{serialisedTickets}");
            if (serialisedTickets != null)
            {
                userTickets = JsonConvert.DeserializeObject<UserTickets>(serialisedTickets);
                
            }
            else
            {
                Console.WriteLine($"{msg}:^No tickets");
            }
            // Try to download tickets from server, if they differ from the locally stored tickets then overwrite local storage.
            var downloadedTickets = await WebService.GetTickets();
            if(downloadedTickets!=null)
            {
                userTickets = downloadedTickets;
            }
            RegenerateQRCodes(msg);
        }

        private void RegenerateQRCodes(string msg)
        {
            Console.WriteLine($"{msg}:^Regenerating QR Codes");
            
            foreach(Ticket ticket in userTickets.userTickets)
            {
                string encodedQRCodeData;
                (byte[] serialisedTimeStamp, byte[] r, byte[] s) signedTimeStamp = WebService.GenerateSignedTimeStamp();
                QRCode QRCodeData = new QRCode(ticket.ServerSignedTicket, signedTimeStamp);
                encodedQRCodeData = JsonConvert.SerializeObject(QRCodeData);
                ticket.QRCode = encodedQRCodeData;
                Console.WriteLine($"{msg}:^Fresh Timestamp: {ticket.QRCode}");
            }
            Console.WriteLine($"{msg}: ^Tickets length BEFORE= {Tickets.Count}");
            Tickets = new ObservableCollection<Ticket>(userTickets.GetItemsAsync(false).Result);
            Console.WriteLine($"{msg}: ^Tickets length AFTER= {Tickets.Count}");
        }

        /*private void DebugGenerateQRCode()
        {
            //Testing QR Code generation: 
            string encodedQRCodeData;
            WebService.InitialiseNewAppSignature();//Only used for debugging
            (byte[] serialisedTimeStamp, byte[] r, byte[] s) signedTimeStamp = WebService.GenerateSignedTimeStamp();
            //Generate QR Code
            byte[] serverSignedTicket = new byte[32];
            QRCode QRCodeData = new QRCode(serverSignedTicket, signedTimeStamp);
            encodedQRCodeData = JsonConvert.SerializeObject(QRCodeData);
            userTickets.AddItemAsync(new Ticket("0", "The Hunna", Genre.Rock, "Hunna Fever", "Watford", "19:00", encodedQRCodeData, (-1,null)));
            SecureStorage.SetAsync("DEBUGPUBLICKEY", SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);//for debugging purposes only
            //System.Threading.Thread.Sleep(5000);//wait 5 seconds
            //Extract data from QR Code
            *//*string scannedEncodedQRCodeData = encodedQRCodeData;
            QRCode scannedQRCodeData = JsonConvert.DeserializeObject<QRCode>(scannedEncodedQRCodeData);
            string decision = WebService.VerifyTimeStamp(scannedQRCodeData.appSignedTimeStamp, SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);
            Console.WriteLine($"£ {decision}");*//*
        }*/

        private async void OnSignOutButtonClicked(object obj)
        {
            //TODO: Code for signing out.
            await Shell.Current.GoToAsync($"//login");
        }

        private async void OnSettingsButtonClicked(object obj)
        {
            await Shell.Current.GoToAsync($"user/{nameof(UserSettingsPage)}");
        }

    }
}
