using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Accounts
{
    public class Account
    {
        public int AccountId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
}