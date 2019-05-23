using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class UserStatusModel
    {
        //Deze model wordt gebruikt om de status van de huidige user bij te houden
        //De properties worden gevuld aan de hand van de huidige sessie
        //Bijvoorbeeld: als de user zich in logt, dan wordt LoggedIn true, en worden de user attributes opgehaald

        
        public bool LoggedIn { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public int userId { get; set; }
        public string adminPriv { get; set; }
    }
}
