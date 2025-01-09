using System;
using System.Collections.Generic;
using System.Text;

namespace csc8208Maui.Models
{
    public class Account
    {
        public string firstName;
        public string secondName;
        public string emailAddress;
        public bool verifier;
        public string appPublicKey;

        public Account(string firstName, string secondName, string emailAddress, bool verifier)
        {
            this.firstName = firstName;
            this.secondName = secondName;
            this.emailAddress = emailAddress;
            this.verifier = verifier;
        }
    }
}
