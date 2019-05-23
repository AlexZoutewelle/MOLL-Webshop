using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Controllers;
using TestWebApp.Models;

namespace TestWebApp.ViewComponents
{
    //ViewComponents worden gebruikt voor partial views
    //UserStatus zal worden gebruikt om data te injecteren in partial views die te maken hebben met de huidige status van een user

    public class UserStatus : ViewComponent
    {
        //Invoke is de entrypoint. Van hieruit kunnen we bepalen welke View we willen instantiëren
        public IViewComponentResult Invoke()
        {
            UserStatusModel userStatus = SessionController.CheckLoggedInStatus(this.HttpContext);

            if(userStatus.LoggedIn == false)
            {
                return View("NotLoggedIn");
            }

            else
            {
                return View("LoggedIn", userStatus);
            }
        }
    }
}
