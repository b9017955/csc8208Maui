using System;
using System.Collections.Generic;
using System.Text;
//using ZXing.Net.Mobile.Forms;

namespace csc8208Maui.Models
{
    public class Ticket
    {
        public string ID { get; set; }
        public string Artist { get; set; }
        public Genre MusicGenre { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }
        public string DoorsOpen { get; set; }
        public string QRCode { get; set;}
        public (int ticket_id, int[] signed_ticket_id) ServerSignedTicket { get; set; }
        public Ticket(string id, string artist, Genre genre, string eventName, string eventLocation, string doorsOpen, string qrcode, (int ticket_id, int[] signed_ticket_id) serverSignedTicket)
        {
            ID = id;
            Artist = artist;
            MusicGenre = genre;
            EventName = eventName;
            EventLocation = eventLocation;
            DoorsOpen = doorsOpen;
            QRCode = qrcode;
            ServerSignedTicket = serverSignedTicket;
        }
    }
}
