using csc8208Maui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csc8208Maui.Services
{
    public class UserTickets : IDataStore<Ticket>
    {
        public readonly List<Ticket> userTickets = new List<Ticket>();
        public async Task<bool> AddItemAsync(Ticket item)
        {
            userTickets.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Ticket item)
        {
            var oldItem = userTickets.Where((Ticket arg) => arg.ID == item.ID).FirstOrDefault();
            userTickets.Remove(oldItem);
            userTickets.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = userTickets.Where((Ticket arg) => arg.ID == id).FirstOrDefault();
            userTickets.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Ticket> GetItemAsync(string id)
        {
            return await Task.FromResult(userTickets.FirstOrDefault(s => s.ID == id));
        }

        public async Task<IEnumerable<Ticket>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(userTickets);
        }
        public void DeleteAllItems()
        {
            userTickets.Clear();
        }
        public async void GenerateFakeData()
        {
            DeleteAllItems();
            AddItemAsync(new Ticket("0", "The Hunna", Genre.Rock, "Hunna Fever", "Watford", "19:00", null, (-1,null)));
            AddItemAsync(new Ticket("1", "Daði Freyr", Genre.Dance, "Eurovision", "Reykjavík", "15:00", null, (-1, null)));
            AddItemAsync(new Ticket("2", "Billie Eilash", Genre.Dance, "Grammys", "Los Angeles", "21:00", null, (-1, null)));
            AddItemAsync(new Ticket("3", "2Pac", Genre.Rap, "He's Back Festival", "Las Vegas", "16:00", null, (-1, null)));
            AddItemAsync(new Ticket("4", "Louis Armstrong", Genre.Jazz, "Prohibition Blues", "New Orleans", "19:00", null, (-1, null)));
            AddItemAsync(new Ticket("5", "Neck Deep", Genre.Rock, "Hospital Festival", "Wrexham", "20:00", null, (-1, null)));
            AddItemAsync(new Ticket("6", "The 1975", Genre.Rock, "Caroline's Birthday Party", "Wilmslow", "23:00", null, (-1, null)));
            AddItemAsync(new Ticket("7", "L’Orange", Genre.HipHop, "BlackBerry Festival", "North Carolina", "8:00", null, (-1, null)));
        }
    }
}