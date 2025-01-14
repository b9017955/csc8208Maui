using System;
using System.Collections.Generic;
using System.Text;

namespace csc8208Maui.Models
{
    public class Account(string firstName, string secondName, string emailAddress, bool verifier)
    {
        public string firstName = firstName;
        public string secondName = secondName;
        public string emailAddress = emailAddress;
        public bool verifier = verifier;
        public string appPublicKey="";
    }
}
