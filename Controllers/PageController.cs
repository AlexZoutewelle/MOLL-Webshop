using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;
using TestWebApp.ElasticSearch.Queries;
using System.IO;
using System.Web;

namespace TestWebApp.Controllers
{
    public class PageController : Controller
    {
        private tbl_servicedata serviceMdl;
        private UserStatusModel userStatusMdl;

        public PageController()
        {
            this.serviceMdl = new tbl_servicedata();
            this.userStatusMdl = new UserStatusModel();
        }

        [HttpGet]
        public IActionResult Register(tbl_userdata user)
        {
            return View(user);
        }

        [HttpPost]
        public IActionResult DoRegister(string UserName, string Password, string FirstName, string LastName, string GenderValue, string Adres, string ZipCode, string DOB, string Phone, string Email)
        {
            tbl_userdata user = new tbl_userdata();
            user.fld_username = UserName;
            user.fld_password = Password;
            user.fld_firstname = FirstName;
            user.fld_lastname = LastName;
            user.fld_gender = GenderValue;
            user.fld_address = Adres;
            user.fld_zipcode = ZipCode;
            user.fld_dateofbirth = DOB;
            user.fld_phonenumber = Phone;
            user.fld_email = Email;

            int emailIsTaken = MollShopContext.CheckIfUserExists(user.fld_email);
            if (emailIsTaken == 0)
            {
                int userNameExistance = MollShopContext.CheckIfUserNameIsTaken(user.fld_username);

                switch (userNameExistance)
                {
                    case 0:
                        user.fld_adminPriv = "N";
                        string activationToken = MollShopContext.RegisterNewUser(user);
                        if (activationToken == "Db Error!")
                        {
                            ViewData["message"] = "Something went wrong on our end. Please contact support.";
                            break;
                        }
                        SendVerificationLink(activationToken, user.fld_email);
                        return View("Login", new LoginModel());
                    case 1:
                        ViewData["message"] = "This user name is already in use!";
                        break;
                    default:
                        ViewData["message"] = "Something went wrong on our end. Please contact support.";
                        break;
                }
                return View("Register", user);
            }

            else
            {
                ViewData["message"] = "This email address has already been registered";
                return View("Register", user);
            }
        }

        [NonAction]
        public void SendVerificationLink(string verificationToken, string emailAddress)
        {
            //Send verification link via email
            int userId = MollShopContext.FindUserIdByEmail(emailAddress);
            string messageBody = @"<body style='box-sizing: border-box;font-family: &quot;Lato&quot;, &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Tahoma, Sans-Serif;width: auto;height: 100vw;margin: 0 0 0 0;'>
    <div id='section_form' style='box-sizing: border-box;background: #f4f4f4;padding-top: 2vw;padding-bottom: 5vw;'>
        <img id='circlelogo' src='https://i.imgur.com/lrkjNdQ.png' style='box-sizing: border-box;width: 8vw;height: auto;margin: auto;display: block;position: relative;top: 20px;z-index: 999;'>
        <div id='rectangle' style='box-sizing: border-box;;width: 40vw;height: auto;background: white;padding: 30px;margin: auto;border-radius: 30px;z-index: 950;'>

            <div class='registrationform' style='box-sizing: border-box;'>
                <table style='box-sizing: border-box;border: 0px solid #ffac35;margin: auto;text-align: center;font-size: 16px;margin-top: 20px;'>
                    <tbody style='box-sizing: border-box;'>
                        <tr style='box-sizing: border-box;'>
                            <td style='box-sizing: border-box;'>
                                <p style='box-sizing: border-box;color: #666666;font-size: 27px;font-weight: 700;font-family: &quot;Lato&quot;, &quot;Lucida Grande&quot;, &quot;Lucida Sans Unicode&quot;, Tahoma, Sans-Serif;margin-bottom: 5%;'>Click the link below to verify your account!</p>
                            </td>
                        </tr>

                        <tr style='box-sizing: border-box;'>
                            <td style='box-sizing: border-box;'>" +
                 "<a href =\"https://localhost:44346/Account/VerifyAccount?token=" + verificationToken + "&userid=" + userId + "\"><button style='box-sizing: border-box;background-color: #ffac35;color: white;padding: 10px 10px;border: none;cursor: pointer;opacity: 0.9;width: 220px;height: 100px;font-size: 25px;font-weight: 700;border-radius: 7px;-moz-transition: all 0.2s ease 0s;-o-transition: all 0.2s ease 0s;transition: all 0.2s ease 0s;margin: 0 auto;text-align: center;'>VERIFY NOW</button> </a>" +

            "";
            string subject = "Verify your account";
            MollShopContext.SendEmail(emailAddress, subject, messageBody);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginModel());
        }

        //Updates the Session to have the user considered not to be logged in
        //Redirects the user to the current page for now
        //
        public IActionResult LogOut()
        {
            HttpContext.Session.Remove("User");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            HttpContext.Session.SetString("LoggedIn", "false");
            return RedirectToAction("HomePage", "Page");
        }

        public IActionResult DoLogin(string Password, string EmailAddress)
        {
            LoginModel loginMdl = new LoginModel(EmailAddress, Password);

            loginMdl = DatabaseController.Login(loginMdl, this.HttpContext);

            if(loginMdl.UserId <= 0)
            {
                return View("Login", loginMdl);
            }
            else
            {
                return RedirectToAction("HomePage", "Page");

            }
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }


        //Requesting a password via email
        public IActionResult RequestPassword(string emailAddress)
        {

            string subject = "Password request";
            tbl_userdata user = MollShopContext.FindUserByEmail(emailAddress);
            if (user.fld_userid == null)
            {
                //wrong emailaddress, reload the page and add the message "Email account not found"
                //For now, redirect to homepage
                return RedirectToAction("HomePage", "Page");
            }

            string firstName = user.fld_firstname;

            string messageBody = "<p>Hi " + firstName + ", You requested your password via email, so here it is: " + user.fld_password + "</p>";
            MollShopContext.SendEmail(emailAddress, "Password request", messageBody);
            return RedirectToAction("HomePage", "Page");
        }


        [HttpGet]
        public IActionResult FavouritesList()
        {
            //Check if user is logged in
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Page", new LoginModel());
            }

            //Fetch the items. type = "FL" if we're looking for favourites, "SC" if were looking for items shopping cart
            List<string> keyList = Request.Cookies.Keys.Where(k => k.StartsWith("FL")).ToList();
            List<OfferedLabourerService> serviceList = Utility.CookieController.fetchItemsInCookies(keyList);

            return View(serviceList);
        }

        [Route("")]
        public IActionResult HomePage()
        {
            //We greet the user with services

            //The 5 most popular
            List<OfferedLabourerService> mostPopular = EsOLSQuery<Object>.mostFavourited();

            //The 5 cheapest
            List<OfferedLabourerService> cheapest = EsOLSQuery<Object>.Cheapest();

            //The 5 Most Bought
            List<OfferedLabourerService> mostBought = EsOLSQuery<Object>.mostBought();

            //5 services from a random category
            //First, create the procedure and method for getting all distinct categories

            Tuple<List<OfferedLabourerService>, List<OfferedLabourerService>, List<OfferedLabourerService>> homePageModel = Tuple.Create(mostPopular, cheapest, mostBought);
            return View(homePageModel);
            //5 Services from a random category
        }

        public IActionResult AdminDashboard()
        {
            return View("Dashboard");
        }

    }
}