using csc8208Maui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace csc8208Maui.Services
{
    public class EventStore : IDataStore<Event>
    {
        readonly List<Event> events= new List<Event>();
        public async Task<bool> AddItemAsync(Event item)
        {
            events.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Event item)
        {
            var oldItem = events.Where((Event arg) => arg.ID == item.ID).FirstOrDefault();
            events.Remove(oldItem);
            events.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = events.Where((Event arg) => arg.ID == id).FirstOrDefault();
            events.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Event> GetItemAsync(string id)
        {
            return await Task.FromResult(events.FirstOrDefault(s => s.ID == id));
        }

        public async Task<IEnumerable<Event>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(events);
        }

        public void DeleteAllItems()
        {
            events.Clear();
        }

        //Should be moved to a unit test?
        public async void GenerateFakeData()
        {
            DeleteAllItems();
            AddItemAsync(new Event("1", "The Hunna", Genre.Rock, "Watford", "19:00"));
            AddItemAsync(new Event("2", "Daði Freyr", Genre.Dance, "Reykjavík", "15:00"));
            AddItemAsync(new Event("0", "Billie Eilash", Genre.Dance, "Los Angeles", "21:00"));
            AddItemAsync(new Event("3", "2Pac", Genre.Rap, "Las Vegas", "16:00"));
            AddItemAsync(new Event("4", "Louis Armstrong", Genre.Jazz, "New Orleans", "19:00"));
            AddItemAsync(new Event("5", "Neck Deep", Genre.Rock, "Wrexham", "20:00"));
            AddItemAsync(new Event("6", "The 1975", Genre.Rock, "Wilmslow", "23:00"));
            AddItemAsync(new Event("7", "L’Orange", Genre.HipHop, "North Carolina", "8:00"));
        }
    }
}
