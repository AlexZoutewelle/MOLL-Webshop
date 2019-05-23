using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class LoginModel
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        //Message to be used to inform the user about their login tries
        //Example: "Your account was found, but the password seems to be wrong.."
        public string Message { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Admin { get; set; }
       

        public LoginModel()
        {

        }
        public LoginModel(string emailaddress, string password)
        {
            EmailAddress = emailaddress;
            Password = password;
        }

        public LoginModel(string message)
        {
            this.Message = message;
        }


    }
}
