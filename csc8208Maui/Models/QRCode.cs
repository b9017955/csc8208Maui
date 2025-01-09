using System;
using System.Collections.Generic;
using System.Text;

namespace csc8208Maui.Models
{
    public class QRCode
    {
        public (int ticket_id, int[] signed_ticket_id) serverSignedTicket { get; set; }
        public (byte[] serialisedTimeStamp, byte[] r, byte[] s) appSignedTimeStamp { get; set; }

        public QRCode((int ticket_id, int[] signed_ticket_id) serverSignedTicket, (byte[] serialisedTimeStamp, byte[] r, byte[] s) appSignedTimeStamp)
        {
            this.serverSignedTicket = serverSignedTicket;
            this.appSignedTimeStamp = appSignedTimeStamp;
        }
    }
}
