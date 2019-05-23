using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class Login
    {
        public Login()
        {

        }
        public Login(string emailaddress)
        {
            EmailAddress = emailaddress;
        }

        public string EmailAddress { get; set; }
    }
}
