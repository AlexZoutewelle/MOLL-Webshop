using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models;
using TestWebApp.Models.Orders;
using TestWebApp.Models.ProductPackage;

//Hier komen alle functies met betrekking tot Accounts van gebruikers
//Bijvoorbeeld: Wijzig een UserName
namespace TestWebApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult AlterTextField(string field, string newText )
        {
            int userId = (int)HttpContext.Session.GetInt32("UserId");

            if(field == "fld_password")
            {
                newText = MollShopContext.SaltNHash(newText);
            }
            MollShopContext.UpdateVarCharField("tbl_userdata", field, newText.ToString(), "fld_UserId", userId);

            return RedirectToAction("MyAccount", "Account");
        }

        public IActionResult ChangeAccount(tbl_userdata user)
        {
            MollShopContext.UpdateRow(user, "fld_userid", (int)HttpContext.Session.GetInt32("UserId"));

            if(user.fld_username != null)
            {
                HttpContext.Session.SetString("UserName", user.fld_username);
            }

            return RedirectToAction("MyAccount", "Account");
        }

        public IActionResult VerifyAccount(string token, int userid)
        {
            tbl_userdata user = new tbl_userdata();
            user.fld_activationcode = token;
            user.fld_userid = userid;
            int verificationResult = MollShopContext.VerifyUser(token, userid);
            switch (verificationResult)
            {
                case 1:
                    ViewData["message"] = "Congratulations, your account is now verified!";

                    break;
                default:
                    ViewData["message"] = "omething went wrong. Please contact support";

                    break;
            }
            return View(user);
        }


        //Pagina voor My Account
        public IActionResult MyAccount()
        {
            //Vraag de useraccount op met een procedure
            //Get User ID from the browser session
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
              return RedirectToAction("Login", "Page", new LoginModel());
            }
            tbl_userdata foundUser = MollShopContext.FindUserById((int)userId);
            return View(foundUser);
        }

        public IActionResult MyOrders()
        {
            //Get the orders bound to the current userId
            //The email
            string userEmail = this.HttpContext.Session.GetString("User");

            //tbl_orders
            Dictionary<string, object> dic = new Dictionary<string, Object>();
            dic.Add("in_givenEmail", userEmail);
            List<Order> orders = ProcedureCall<Order>.returnResultList(dic, "util_GetOrders");

            //Instantiate lists
            List<List<tbl_orderhistory>> orderHistories = new List<List<tbl_orderhistory>>();

            List<tbl_orderstatus> orderStatuses = new List<tbl_orderstatus>();

            List<OfferedLabourerService> olsList = new List<OfferedLabourerService>();

            
            //Get all the relevant data
            for(int i = 0; i < orders.Count; i++)
            {
                Dictionary<string, Object> dic2 = new Dictionary<string, Object>();
                dic2.Add("in_orderid", orders[i].fld_OrderId);
                //tbl orderhistory
                List<tbl_orderhistory> orderhistories = ProcedureCall<tbl_orderhistory>.returnResultList(dic2, "util_GetOrderHistories").OrderByDescending(s => s.fld_ActionDate).ToList();
                //tbl_orderstatus
                tbl_orderstatus orderstatus = ProcedureCall<tbl_orderstatus>.ExecuteReader(dic2, "util_GetOrderStatuses");
                //tbl_offeredlabourerservice
                OfferedLabourerService ols = EsOLSQuery<OfferedLabourerService>.findByOfferedServiceId(orders[i].fld_OfferedServiceId);

                //Add it all to their respectiv lists
                orderStatuses.Add(orderstatus);
                olsList.Add(ols);
                orderHistories.Add(orderhistories);
            }

            
            Tuple<List<Order>, List<tbl_orderstatus>, List<List<tbl_orderhistory>>, List<OfferedLabourerService>> tuple = Tuple.Create(orders, orderStatuses, orderHistories, olsList);
            return View(tuple);
        }
    }
}