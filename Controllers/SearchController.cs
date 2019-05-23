using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.Controllers
{
    //De search controller class is verantwoordelijk voor het renderen van paginas
    //Het verzamelt alle search resultaten afkomstig van de ElasticSearch query classes en funnelt het tot de uiteindelijke View
    public class SearchController : Controller
    {
        [HttpGet]
        public IActionResult SearchResult(OLSSearchModel ols)
        {
            //Deze Action rendert het resultaat van een search
            //Eerst verkrijgt het variabelen afkomstig van de searchform. Die stuurt ie door naar de ESQuery classes
            //Vervolgens verkrijgt hij dan van die ESQuery classes een package. Die gebruikt hij om zichzelf weer te renderen
            //List<OfferedLabourerService> package = EsOLSQuery<string>.mostFavourited();

            System.Diagnostics.Debug.WriteLine("Start Search: " + DateTime.Now.AddDays(-20));
            List<OfferedLabourerService> newPackage = EsOLSQuery<OfferedLabourerService>.generalSearchQuery(ols, 20);
            System.Diagnostics.Debug.WriteLine("Search completed: " + DateTime.Now.AddDays(-20));

            return PartialView(newPackage);
        
        }

        public IActionResult OfferedServiceSearchForm()
        {
            //Deze Action rendert De OfferedService SearchForm
            //De bedoeling is dat deze view de variabelen zal oppikken en versturen naar de ESQuery classes

            return View();
        }

        public IActionResult Search(List<OfferedLabourerService> package)
        {
            //Deze Action rendert de Search pagina waarop de SearchResult en de SearchForm zitten.
            //Deze Action kan door iedereen gecalled worden en zal dan een gespecializeerde Search View renderen.

            return View(package);
        }

        public IActionResult SimpleSearch(string simpleQuery)
        {
            List<OfferedLabourerService> newPackage = EsOLSQuery<OfferedLabourerService>.simpleSearch(simpleQuery);
            return View("Search", newPackage);
        }


    }
}