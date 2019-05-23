using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayPalPayment.PayPal;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models.Orders;
using TestWebApp.ElasticSearch;

namespace TestWebApp.Controllers
{
    public class ShoppingCartController : Controller
    {
        public IActionResult ShoppingCart()
        {
            List<OfferedLabourerService> serviceList = new List<OfferedLabourerService>();

            //Check if we're logged in. 
            bool loggedIn = SessionController.returnLoggedIn(this.HttpContext);

            if (!loggedIn)
            {
                //Not logged in, get products from cookies
                List<string> keyList = Request.Cookies.Keys.Where(k => k.StartsWith("SC")).ToList();
                serviceList = Utility.CookieController.fetchItemsInCookies(keyList);
            }

            else
            {
                //Logged in, get products from database
                List<Object> offeredServiceIds = MollShopContext.GetShoppingCartItems((int)HttpContext.Session.GetInt32("UserId"));
                serviceList = new List<OfferedLabourerService>();

                foreach (Object id in offeredServiceIds)
                {
                    serviceList.Add(ElasticSearch.Queries.EsOLSQuery<object>.findByOfferedServiceId((int)id));
                }
            }


            PayPalConfig payPalConfig = PayPalService.getPayPalConfig();
            ViewBag.payPayConfig = payPalConfig;

            return View(serviceList);
        }




        //This Action is called from the initial shopping cart page
        //The user will click this when they are ready to purchase their products
        //It will return 1 of 2 views, depending on the users' login status
        public IActionResult PaymentAuthentication(string orders)
        {
            //First check if there are any orders. If not, nothing should happen. Refresh the page
            if (orders == null)
            {
                return RedirectToAction("ShoppingCart", "ShoppingCart");
            }

            //Retrieve the user status to check whether the user is logged in or not
            UserStatusModel userStatus = SessionController.CheckLoggedInStatus(this.HttpContext);

            //Put the orders in the browser session. This refreshes the list, so if the user has altered their shopping cart, it'll always be up to date when entering the ordering sequence
            SessionController.refreshOrderList(orders, this.HttpContext);





            if (userStatus.LoggedIn)
            {
                //User is logged in already, so return the PaymentMethod view
                List<OfferedLabourerService> olsList = ParseOrdersToOLS();

                tbl_userdata user = MollShopContext.FindUserById((int)this.HttpContext.Session.GetInt32("UserId"));
                Tuple<List<OfferedLabourerService>, tbl_userdata> tuple = Tuple.Create(olsList, user);

                return View("OrderSpecification", tuple);
            }

            else
            {
                //User is not logged in. return the PaymentAuthentication view
                return View();
            }
        }

        //This action does the authentication for registered users that are about to make a purchase
        public IActionResult DoAuthentication(string fld_emailaddress, string fld_password)
        {
            LoginModel loginMdl = new LoginModel(fld_emailaddress, fld_password);
            loginMdl = DatabaseController.Login(loginMdl, this.HttpContext);



            ViewBag.Message = loginMdl.Message;

            if (loginMdl.UserId <= 0)
            {
                return View("PaymentAuthentication", this.HttpContext.Session.GetString("ORD"));
            }
            else
            {
                this.HttpContext.Session.SetString("OrderMail", loginMdl.EmailAddress);

                tbl_userdata user = MollShopContext.FindUserById(loginMdl.UserId);
                return RedirectToAction("OrderSpecification", user);
            }
        }

        //This action gets called when the user decides to not use an account when making their purchase
        public IActionResult UnregisteredAuthentication(string fld_emailaddress)
        {
            this.HttpContext.Session.SetString("OrderMail", fld_emailaddress);

            tbl_userdata user = new tbl_userdata();
            user.fld_email = fld_emailaddress;
            return RedirectToAction("Orderspecification", user);
        }

        public IActionResult OrderSpecification(tbl_userdata user)
        {
            //This view will show the order the user is about to make
            //They can make last minute changes: for example: a different mailing address, a target send date, etc
            //By clicking the proceed button, the user will be directed to the PaymentMethod view

            //List all the services

            List<OfferedLabourerService> olsList = ParseOrdersToOLS();

            Tuple<List<OfferedLabourerService>, tbl_userdata> tuple = Tuple.Create(olsList, user);
            return View(tuple);
        }

        public IActionResult PaymentMethod(string fld_email, string fld_firstname, string fld_lastname, string fld_address, string fld_zipcode)
        {
            //The variables received from this form will be used to create the confirmation email later on
            HttpContext.Session.SetString("FORM", fld_firstname + ";" + fld_lastname + ";" + fld_address + ";" + fld_zipcode + ";" + fld_email);


            //This view will show the users their options for paying
            //In reality, the user would be able to choose between IDEAL, PayPal, etc. However, we only have PayPal

            List<OfferedLabourerService> olsList = ParseOrdersToOLS();

            PayPalConfig payPalConfig = PayPalService.getPayPalConfig();
            ViewBag.payPayConfig = payPalConfig;


            return View(olsList);
        }

        public IActionResult CreateOrders(string fld_email)
        {
            try
            {
                //Create the orders

                List<int> orderIds = ParseOrdersToList();
                Order order = new Order();

                order.fld_email = fld_email;
                if (fld_email == null)
                {
                    //The OrderMail Session Key is null. This means the user is already logged in, and did not need to save that key
                    order.fld_email = HttpContext.Session.GetString("User");
                }

                //Insert the Orders in the database
                foreach (int orderId in orderIds)
                {
                    order.fld_OfferedServiceId = orderId;
                    MollShopContext.CreateRow(order, "tbl_orders");

                    //If the user is logged in, we need to remove the orders from their database shopping cart
                    if (fld_email == null)
                    {
                        DatabaseController.RemoveFromShoppingCart(orderId, (int)HttpContext.Session.GetInt32("UserId"));
                    }
                }

                //With this, we have created the orders, and the user is now in the process of paying for their products. We can delete the orders.

                //First, we move the orders in the Session to a new Key. This way, we can still summarize the Orders if the order was successful later, but at the same time
                //We prevent the orders from being made multiple times over by user accident

                //Put the Ordered items in "PUR", for "Purchased
                this.HttpContext.Session.SetString("PUR", this.HttpContext.Session.GetString("ORD"));

                //Remove the orders from the Session
                this.HttpContext.Session.Remove("ORD");

                //Remove the ShoppingCart items from the Cookies
                List<string> items = HttpContext.Request.Cookies.Keys.Where(s => s.StartsWith("SC")).ToList();

                foreach (string item in items)
                {
                    Response.Cookies.Delete(item);
                }

            }

            catch (Exception e)
            {

            }

            return View("Success");
        }

        public async Task<IActionResult> Success()
        {
            //This action returns the result of the payment.
            //This is when the order will receive it's first update: it's either payed or encountered an error.
            var result = PDTHolder.Success(Request.Query["tx"].ToString());

            //Update the order status and history, update the offeredservice


            //Get previously entered order information
            string form = HttpContext.Session.GetString("FORM");
            char separator = ';';
            string[] formVars = form.Split(separator);

            //Send a confirmation email
            await ConstructOrderVerificationMailAsync(formVars);


            string email = formVars[4];
            List<int> offeredServiceIds = ParsePursToList();

            foreach(int olsId in offeredServiceIds)
            {

                //Fetch the order id
                int orderId = MollShopContext.FindOrderId(olsId, email);
                
                
                //Insert a new order history
                tbl_orderhistory history = new tbl_orderhistory();
                history.fld_ActionDate = DateTime.Now;
                history.fld_lastAction = "Paid order";
                history.fld_orderstatus = "Sent";
                history.fld_orderid = orderId;

                MollShopContext.CreateRow(history, "tbl_orderhistory");

                //Insert a new order status
                tbl_orderstatus orderStatus = new tbl_orderstatus();
                orderStatus.fld_dateOrdered = DateTime.Now;
                orderStatus.fld_orderid = orderId;
                orderStatus.fld_targetDeliveryDate = DateTime.Now.AddDays(7);
                orderStatus.fld_DateUpdated = DateTime.Now;
                MollShopContext.CreateRow(orderStatus);
                
                //Set the availability of the service to 'N'

                //ElasticSearch
                EsUpdater<OfferedLabourerService>.UpdateField("" + olsId, "fld_stillavailable", 'N');

                //Database
                tbl_offeredservicesdata os = new tbl_offeredservicesdata();
                os.fld_stillavailable = 'N';

                MollShopContext.UpdateRow(os, "fld_OfferedServiceId", olsId);
            }



            return View("Success");
        }

        [NonAction]
        public async Task ConstructOrderVerificationMailAsync(string[] formVars)
        {
            //Get form information



            //Get labourer and service information
            List<OfferedLabourerService> purList = ParsePursToOls();

            Tuple<List<OfferedLabourerService>, string[]> emailModel = Tuple.Create(purList, formVars);

            string messageBody = await this.RenderViewAsync("OrderConfirmMail", emailModel);

            MollShopContext.SendEmail(formVars[4], "Order confirmation", messageBody);

           
        }

        [NonAction]
        public List<int> ParsePursToList()
        {
            try
            {
                string orders = "";
                if (this.HttpContext.Session.GetString("PUR") == null)
                {
                    orders = this.HttpContext.Session.GetString("ORD");
                }
                else
                {
                    orders = this.HttpContext.Session.GetString("PUR").Substring(1);
                }

                char separator = ' ';
                string[] orderIds = orders.Split(separator);

                List<int> olsList = new List<int>();

                foreach (string id in orderIds)
                {
                    int olsId = Convert.ToInt32(id);
                    olsList.Add(olsId);
                }

                return olsList;
            }
            catch(Exception e)
            {
                return new List<int>();
            }


        }

        [NonAction]
        public List<OfferedLabourerService> ParsePursToOls()
        {
            string pur = this.HttpContext.Session.GetString("PUR");
            string purs = pur.Substring(1);
            char separator = ' ';
            string[] orderIds = purs.Split(separator);

            List<OfferedLabourerService> olsList = new List<OfferedLabourerService>();

            foreach (string id in orderIds)
            {
                int olsId = Convert.ToInt32(id);
                olsList.Add(EsOLSQuery<OfferedLabourerService>.findByOfferedServiceId(olsId));

            }
            return olsList;
        }

        [NonAction]
        public List<OfferedLabourerService> ParseOrdersToOLS()
        {
            //Add exception handler!
            string orders = this.HttpContext.Session.GetString("ORD").Substring(1);
            char separator = ' ';
            string[] orderIds = orders.Split(separator);

            List<OfferedLabourerService> olsList = new List<OfferedLabourerService>();

            double cost = olsList.Sum(s => s.fld_cost);

            foreach (string id in orderIds)
            {
                int olsId = Convert.ToInt32(id);
                olsList.Add(EsOLSQuery<OfferedLabourerService>.findByOfferedServiceId(olsId));

            }
            return olsList;
        }

        [NonAction]
        public List<int> ParseOrdersToList()
        {
            string orders = this.HttpContext.Session.GetString("ORD").Substring(1);
            char separator = ' ';
            string[] orderIds = orders.Split(separator);

            List<int> olsList = new List<int>();

            foreach(string id in orderIds)
            {
                int olsId = Convert.ToInt32(id);
                olsList.Add(olsId);
            }

            return olsList;

        }

        public void AddToDbShoppingCart(int fld_offeredserviceid, int fld_userid)
        {
            DatabaseController.AddtoShoppingCart(fld_offeredserviceid, fld_userid);
        }
    }
}