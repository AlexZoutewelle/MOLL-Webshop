using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.Controllers
{
    public static class SessionController
    {
        public static UserStatusModel CheckLoggedInStatus(HttpContext context)
        {
            //Get the browser session from the accessor
            ISession session = context.Session;

            //Ready the value for the following bool returning function
            byte[] value = { };

            //check if value for loggedIn exists
            bool loggedIn = session.TryGetValue("LoggedIn", out value);

            //Ready the user status model
            UserStatusModel userStatusMdl = new UserStatusModel();
            userStatusMdl.LoggedIn = false;

            if (loggedIn)
            {
                //It exists, which means the user has logged in before, and we can use the session value
                string isUserLoggedIn = session.GetString("LoggedIn");
                if (isUserLoggedIn == "true")
                {
                    //User is currently logged in
                    userStatusMdl.EmailAddress = session.GetString("User");
                    userStatusMdl.Username = session.GetString("UserName");
                    userStatusMdl.userId = (int)session.GetInt32("UserId");
                    userStatusMdl.LoggedIn = true;
                    userStatusMdl.adminPriv = session.GetString("Admin");

                }
                else if (isUserLoggedIn == "false")
                {
                    //User has logged out at some point
                }
                return userStatusMdl;
            }
            //User has never logged in before during this session
            return userStatusMdl;

        }

        public static bool returnLoggedIn(HttpContext context)
        {
            ISession session = context.Session;

            byte[] value = { };

            bool loggedIn = session.TryGetValue("LoggedIn", out value);

            if (loggedIn)
            {
                if (session.GetString("LoggedIn") == "true")
                {
                    return true;
                }
            }

            return false;
        }

        public static void refreshOrderList(string serviceList, HttpContext context)
        {
            context.Session.SetString("ORD", serviceList);
        }
    }
}