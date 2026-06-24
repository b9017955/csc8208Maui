using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math;
//using ZXing.Net.Mobile.Forms;

namespace csc8208Maui.Models
{
    public class Ticket
    {
        public int ID { get; set; }
        public string Artist { get; set; }
        public Genre MusicGenre { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public string DoorsOpen { get; set; }
        public string QRCode { get; set;}
        //BigInteger[] encoded as Base64 string
        public string ServerSignedTicket { get; set; }
        public Ticket(int id, string artist, Genre genre, string eventName, string eventLocation, string doorsOpen, string serverSignedTicket)
        {
            ID = id;
            Artist = artist;
            MusicGenre = genre;
            EventName = eventName;
            EventLocation = eventLocation;
            DoorsOpen = doorsOpen;
            ServerSignedTicket = serverSignedTicket;
        }
    }
}
