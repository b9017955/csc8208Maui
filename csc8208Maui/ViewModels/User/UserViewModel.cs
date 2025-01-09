   
using csc8208Maui.Views.User;
using csc8208Maui.Services;
using csc8208Maui;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using csc8208Maui.Models;
//using ZXing.Net.Mobile.Forms;
//using ZXing.QrCode;
//using ZXing;
//using ZXing.Common;
using System.Linq;
using Newtonsoft.Json;
//using Plugin.Toast;
using System.Diagnostics;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices.Sensors;

namespace csc8208Maui.ViewModels
{
    //There will be some code duplication between this and the verifier viewmodel e.g. both need to be able to logout.
    class UserViewModel : BaseViewModel
    {
        //Todo: compare this viewmodel with UserAccountViewModel and remove duplicate code FROM THIS VIEWMODEL
        private UserTickets tickets = new UserTickets();
        //public ObservableCollection<Ticket> Tickets { get; set; }
        private EventStore events = new EventStore();
        public ObservableCollection<Event> Events { get; set; }
        public ObservableCollection<Event> FeaturedEvent { get; set; }
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
                if (selectedEvent != null) BuyTicket(selectedEvent);
            }
        }
        public Command LogoutCommand { get; }
        public Command SettingsCommand { get; }
        //public EncodingOptions BarcodeOptions => new EncodingOptions() { Height = 300, Width = 300, PureBarcode = true };
        public UserViewModel()
        {
            LogoutCommand = new Command(OnSignOutButtonClicked);
            SettingsCommand = new Command(OnSettingsButtonClicked);

            events.GenerateFakeData();
            //GenerateEvents();
            //SecureStorage.Remove("tickets");//DEBUG CODE
            //Tickets = new ObservableCollection<Ticket>(tickets.GetItemsAsync(false).Result);
            
            Events = new ObservableCollection<Event>(events.GetItemsAsync(false).Result);
            FeaturedEvent = new ObservableCollection<Event>();
            FeaturedEvent.Add(Events.FirstOrDefault());
        }

        private async void BuyTicket(Event selectedEvent)
        {
            Console.WriteLine("Starting Timer for buy ticket...");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            (int id, int[] signedTicket) downloadedTicket;
            try
            {
                Console.WriteLine($"Selected Event ID: {selectedEvent.ID}");
                downloadedTicket = await WebService.BuyTicket(selectedEvent.ID);
                Console.WriteLine($"DOWNLOADED TICKET: {downloadedTicket}");
            }
            catch
            {
                Console.WriteLine("ERROR COMMUNICATING WITH SERVER");
                SelectedEvent = null;
                return;
            }
            
            //Debug Code==========
            //downloadedTicket = "TICKET";
            
            //====================

            if (downloadedTicket != (-1,null))
            {
                Ticket newTicket = new Ticket("0", selectedEvent.Artist, selectedEvent.MusicGenre, "EVENT_NAME", selectedEvent.Location, selectedEvent.DoorsOpen, "", downloadedTicket);//Ticket.ID is unneccessary in the app, the server does not send the app the ticket ID. The ticket is uniquely identified by the ServersSignature(Hash(Ticket)) 
                tickets.AddItemAsync(newTicket);
                var serialisedTickets = JsonConvert.SerializeObject(tickets);
                Console.WriteLine($"^INPUT_serialisedTickets:{serialisedTickets}");
                SecureStorage.SetAsync("tickets", serialisedTickets);
                //CrossToastPopUp.Current.ShowToastSuccess($"Successfully purchased ticket to see {selectedEvent.Artist}");
            }
            else
            {
                Console.WriteLine("AUTHENTICATION ERROR");
            }
            stopwatch.Stop();
            Console.WriteLine($"Time taken to Buy Ticket: {stopwatch.Elapsed.TotalMilliseconds} ms");
            SelectedEvent = null;
        }

        private async void GenerateEvents()
        {
            EventStore downloadedEventStore = await WebService.GetEvents();
            if (downloadedEventStore != null)
            {
                events = downloadedEventStore;
            }
            else
            {
                //CrossToastPopUp.Current.ShowToastWarning("Unable to retrieve data from server");
            }
        }

        private async void OnSignOutButtonClicked(object obj)
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
            await Shell.Current.GoToAsync($"user/{nameof(UserSettingsPage)}");
        }
    }
}

