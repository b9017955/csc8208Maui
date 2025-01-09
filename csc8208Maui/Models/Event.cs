using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Devices.Sensors;

namespace csc8208Maui.Models
{
    public class Event
    {
        public string ID { get; set; }
        public string Artist { get; set; }
        public Genre MusicGenre { get; set; }
        public string Location { get; set; }
        public string DoorsOpen { get; set; }
        public Event(string id, string artist, Genre genre, string location, string doorsOpen)
        {
            ID = id;
            Artist = artist;
            MusicGenre = genre;
            Location = location;
            DoorsOpen = doorsOpen;
        }
    }
}
