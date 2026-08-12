using csc8208Maui.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csc8208Maui.Services
{
    public class UserTicketStore
    {
        public UserTicketStore()
        {
            //retrieve tickets from storage
            userTickets = [];
            RetrieveFromSecureStorage();
        }
        public void RetrieveFromSecureStorage()
        {
            string serialisedTickets = SecureStorage.GetAsync("tickets").Result;
            if (serialisedTickets is not null)
            {
                Console.WriteLine($"Retrieved tickets from secure storage:^{serialisedTickets}");
                try
                {
                    userTickets = JsonConvert.DeserializeObject<List<Ticket>>(serialisedTickets);
                }
                catch
                {
                    Console.WriteLine("Local ticket data corrupted!");
                }
                
            }
            else
            {
                Console.WriteLine("No tickets found in secure storage!");
            }
        }
        public void SaveToSecureStorage()
        {
            var serialisedTickets = JsonConvert.SerializeObject(userTickets);
            Console.WriteLine($"^INPUT_serialisedTickets:{serialisedTickets}");
            SecureStorage.SetAsync("tickets", serialisedTickets);
        }
        private List<Ticket> userTickets; 
        public async Task<bool> AddItemAsync(Ticket item)
        {
            userTickets.Add(item);
            SaveToSecureStorage();

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Ticket item)
        {
            var oldItem = userTickets.FirstOrDefault((Ticket arg) => arg.ServerSignedTicket == item.ServerSignedTicket);
            userTickets.Remove(oldItem);
            userTickets.Add(item);
            SaveToSecureStorage();

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(Ticket ticket)
        {
            var oldItem = userTickets.FirstOrDefault((Ticket arg) => arg.ServerSignedTicket == ticket.ServerSignedTicket);
            userTickets.Remove(oldItem);
            SaveToSecureStorage();

            return await Task.FromResult(true);
        }

        public async Task<Ticket> GetItemAsync(Ticket ticket)
        {
            return await Task.FromResult(userTickets.FirstOrDefault(s => s.ServerSignedTicket == ticket.ServerSignedTicket));
        }

        public async Task<IEnumerable<Ticket>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(userTickets);
        }
        public void DeleteAllItems()
        {
            userTickets.Clear();
            SaveToSecureStorage();
            Console.WriteLine("LOCAL TICKETS HAVE BEEN DELETED.");
        }
        public async void GenerateFakeData()
        {
            DeleteAllItems();
            AddItemAsync(new Ticket("The Hunna", Genre.Rock, "Hunna Fever", "Watford", "19:00", ""));
            AddItemAsync(new Ticket("Daði Freyr", Genre.Dance, "Eurovision", "Reykjavík", "15:00", ""));
            AddItemAsync(new Ticket("Billie Eilash", Genre.Dance, "Grammys", "Los Angeles", "21:00", ""));
            AddItemAsync(new Ticket("2Pac", Genre.Rap, "He's Back Festival", "Las Vegas", "16:00", ""));
            AddItemAsync(new Ticket("Louis Armstrong", Genre.Jazz, "Prohibition Blues", "New Orleans", "19:00", ""));
            AddItemAsync(new Ticket("Neck Deep", Genre.Rock, "Hospital Festival", "Wrexham", "20:00", ""));
            AddItemAsync(new Ticket("The 1975", Genre.Rock, "Caroline's Birthday Party", "Wilmslow", "23:00", ""));
            AddItemAsync(new Ticket("L’Orange", Genre.HipHop, "BlackBerry Festival", "North Carolina", "8:00", ""));
        }
    }
}