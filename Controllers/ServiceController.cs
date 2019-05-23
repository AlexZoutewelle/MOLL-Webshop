using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using MySql.Data.MySqlClient;
using TestWebApp.Models;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.Controllers
{
    public class ServiceController : Controller
    {
        string connectionString = "server=84.246.4.143;port=9139;Database=koster3mollshop;Uid=Koster3molladmin;Pwd=pi9!rtd@;SslMode=none;";

        [HttpGet]
        public ActionResult Index()
        {
            //Voorlopig is dit nog mostFavourited.
            List<OfferedLabourerService> package = EsOLSQuery<string>.mostFavourited();

            //return View(package)
            return View(package);
        }


        public ActionResult Details(int offeredserviceid)
        {
            //ID meegeven

            OfferedLabourerService package = EsOLSQuery<string>.findByOfferedServiceId(offeredserviceid);


            return PartialView(package);
            
        }

        public IActionResult MiniDetails(int id)
        {

            return View();
        }

        public IActionResult DetailsByModel(OfferedLabourerService model)
        {
            return View("Details", model);
        }

        
        public IActionResult AddToFavourites(int fld_serviceid)
        {

            tbl_servicedata foundService = MollShopContext.FindServiceById(fld_serviceid);

            return Ok();
        }


        public IActionResult fixDatabase()
        {
            List<OfferedLabourerService> packages = EsOLSQuery<OfferedLabourerService>.getAll();

            //Nu willen we alle services en labourers DISTINCT pakken

            //Pak alle services van de OLS
            var services = (from ols in packages
                            orderby ols.fld_offeredserviceid ascending
                            group ols by ols.fld_offeredserviceid into osGroup

                            select osGroup.First()).ToList();

            //Maak van iedere gevonden service een service
            tbl_offeredservicesdata os = new tbl_offeredservicesdata();

            foreach(var item in services)
            {
                os.fld_addedtowishlist = item.fld_addedtowishlist;
                os.fld_area = item.fld_area;
                os.fld_cost = item.fld_cost / 100;
                os.fld_labourerid = item.fld_labourerid;
                os.fld_offeredserviceid = item.fld_offeredserviceid;
                os.fld_serviceid = item.fld_serviceid;
                os.fld_timefirst = item.fld_timefirst;
                os.fld_timelast = item.fld_timelast;
                os.fld_timesbought = item.fld_timesbought;
                os.fld_stillavailable = item.fld_stillavailable;
                  

                int id = MollShopContext.CreateRow(os, "tbl_offeredservicesdata");
                Console.WriteLine();
            }



            return Ok();
        }
    }
}