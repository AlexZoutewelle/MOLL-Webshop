using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestWebApp.Controllers;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.Utility
{
    //Class voor cookie functies
    public class CookieController : Controller
    {
        public CookieController()
        {
        }

        public IActionResult AddToFavourites(int fld_offeredserviceid)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddSeconds(200);

            //De message die we de user geven gaat afhangen van de volgende try catch blokken:

            try
            {
                //De key voor de cookie
                string currentKey = "FL" + fld_offeredserviceid;
                //Check eerst of de service al in de favorites zit
                int sameKey = Request.Cookies.Keys.Count(k => k.Equals(currentKey));
                if (sameKey > 0)
                {
                    //Service zit al in favorites. Doe niets
                    ViewBag.FLResultMessage = "<p>Again?</p>";
                    return View();
                }
                //Service zit nog niet in favorites
                Response.Cookies.Append(currentKey, "1", options);
                //Response.WriteAsync("<script>alert('test')</script>");

                ViewBag.FLResultMessage = "<p>Success!</p>";

                //Update the OLS in ElasticSearch
                OfferedLabourerService ols = EsOLSQuery<OfferedLabourerService>.findByOfferedServiceId(fld_offeredserviceid);
                ols.fld_addedtowishlist = ols.fld_addedtowishlist + 1;

                //Because ElasticSearch does not support decimal numbers, we must multiply the cost by a 100
                ols.fld_cost = ols.fld_cost * 100;
                ElasticSearch.EsUpdater<OfferedLabourerService>.UpsertDocument(ols, "moll_ols", "OLS", fld_offeredserviceid);

                return View();
            }
            catch(Exception e)
            {
                ViewBag.FLResultMessage = "<p>Success!</p>";

                return View();
            }
        }

        public IActionResult RemoveFromFavourites(int fld_offeredserviceid)
        {
            string currentKey = "FL" + fld_offeredserviceid;

            Response.Cookies.Delete(currentKey);

            return RedirectToAction("FavouritesList", "Page");
        }

        public IActionResult AddtoShoppingCart(int fld_offeredserviceid)
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(14);

            //De key voor de cookie
            string currentKey = "SC" + fld_offeredserviceid;

            //Check eerst of de service al in de shoppingcart zit
            int sameKey = Request.Cookies.Keys.Count(k => k.Equals(currentKey));
            if (sameKey > 0)
            {
                //Service zit al in de shoppingcart. Verhoog de kwantiteit met 1
                int quantity = Convert.ToInt16(Request.Cookies[currentKey]);
                quantity++;
                Response.Cookies.Append(currentKey, quantity.ToString(), options);

                //Eigenlijk horen we dit niet te doen. Als het item al in de shopping cart zit, dan moet dat worden aangegeven.
                //Vervolgens moet de user een search kunnen doen naar services die erop lijken of zelfs hetzelfde zijn
                //We kunnen dat op verschillende manieren doen nu
                //We kunnen de offeredservices bekijken met dezelfde serviceId
                //We kunnen de offeredservices queryen op basis van naam en description

                return RedirectToAction("Details", "Service", new { offeredserviceid = fld_offeredserviceid });
            }
            //Service zit nog niet in favorites
            Response.Cookies.Append(currentKey, "1", options);
            //Response.WriteAsync("<script>alert('test')</script>");

            return RedirectToAction("Details", "Service", new { offeredserviceid = fld_offeredserviceid });
        }

        public IActionResult RemoveFromShoppingcart(int fld_offeredserviceid)
        {
            //Check if user is logged in
            UserStatusModel status = SessionController.CheckLoggedInStatus(this.HttpContext);

            if(status.LoggedIn == true)
            {
                //If user is logged in, remove the item from the database as well
                DatabaseController.RemoveFromShoppingCart(fld_offeredserviceid, status.userId);

            }
            string currentKey = "SC" + fld_offeredserviceid;
            
            Response.Cookies.Delete(currentKey);
          
            return RedirectToAction("ShoppingCart", "ShoppingCart");
        }

        [NonAction]
        public static List<OfferedLabourerService> fetchItemsInCookies(List<string> keyList)
        {
            List<OfferedLabourerService> packageList = new List<OfferedLabourerService>();
            foreach (string key in keyList)
            {
                int offeredServiceId = Convert.ToInt32(key.Substring(2));

                OfferedLabourerService ols = EsOLSQuery<string>.findByOfferedServiceId(offeredServiceId);
                packageList.Add(ols);
            }
            return packageList;
        }
    }
}
