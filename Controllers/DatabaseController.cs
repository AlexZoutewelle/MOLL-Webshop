using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using TestWebApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using TestWebApp.Models.Orders;

namespace TestWebApp.Controllers
{
    public static class DatabaseController
    {
        public static string AddtoShoppingCart(int fld_offeredserviceid, int fld_userid)
        {

            //Check eerst of de service al in de shoppingcart zit
            //zet het voor nu ff op false
            bool foundService = MollShopContext.CheckShoppingCartItem(fld_offeredserviceid, fld_userid);

            if (!foundService)
            {
                //Service zit nog niet in de shoppingcart. 
                ShoppingCartItem item = new ShoppingCartItem();
                item.fld_offeredServiceId = fld_offeredserviceid;
                item.fld_UserId = fld_userid;
                try
                {

                    MollShopContext.CreateRow(item, "tbl_shoppingcart");
                }

                catch(Exception e)
                {

                }
            }

            return "hey";
        }

        public static string RemoveFromShoppingCart(int fld_offeredserviceid, int fld_userId)
        {
            try
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("in_TargetUser", fld_userId);
                dic.Add("in_TargetItem", fld_offeredserviceid);

                ProcedureCall<object>.ExecuteNonQuery(dic, "util_DeleteShoppingCartItem");

                return "Done";
            }

            catch(Exception e)
            {
                return "Failed";
            }
        }

        [NonAction]
        public static LoginModel Login(LoginModel loginMdl, HttpContext context)
        {
            loginMdl = MollShopContext.UserLogin(loginMdl);

            int result = loginMdl.UserId;
            if (result <= 0)
            {
                //Authentication unsuccessful
                switch (result)
                {
                    case 0:
                        loginMdl.Message = "Wrong password!";
                        break;
                    case -1:
                        loginMdl.Message = "Account has not been found!";
                        break;
                    case -2:
                        loginMdl.Message = "Something went wrong on our end. Please contact support.";
                        break;
                    case -3:
                        loginMdl.Message = "Hello, " + loginMdl.UserName + ". It seems you have not yet activated your account. Please check your mail!";
                        break;
                    default:

                        break;
                }
            }

            else
            {
                //Authentication was successful
                context.Session.SetInt32("UserId", loginMdl.UserId);
                context.Session.SetString("User", loginMdl.EmailAddress);
                context.Session.SetString("UserName", loginMdl.UserName);
                context.Session.SetString("LoggedIn", "true");
                context.Session.SetString("Admin", loginMdl.Admin.ToString());

                //If the user has logged in, we need to check if they have any items added to their Shopping cart cookies, and add it to the database

                List<string> items = context.Request.Cookies.Keys.Where(s => s.StartsWith("SC")).ToList();

                foreach (string item in items)
                {
                    int fld_offeredserviceid = Convert.ToInt32(item.Substring(2));
                    AddtoShoppingCart(fld_offeredserviceid, loginMdl.UserId);
                }
            }

            return loginMdl;
        }
    }
}