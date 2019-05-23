using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;
using TestWebApp.Pages;

namespace TestWebApp.Controllers
{
    public class LabourerController : Controller
    {
        string connectionString = "server=84.246.4.143;port=9139;Database=koster3mollshop;Uid=Koster3molladmin;Pwd=pi9!rtd@;SslMode=none;";

        public IActionResult Index()
        {
            //Return alle Labourers in een List
            //Deze shit gebruiken we niet meer en is nutteloos nu
            return View("/Index");
        }

        [HttpGet]
        public IActionResult Write()
        {
            return View();
        }

        //Note, je kan dit nu alleen callen met als actie = DetailsById
        [ActionName("DetailsById")]
        public IActionResult Details(string labourerId)
        {

            tbl_labourerdata labourer = MollShopContext.FindLabourerById(labourerId);
            return View("Details", labourer);

        }

        [ActionName("DetailsByModel")]
        public IActionResult Details(OfferedLabourerService package)
        {
            tbl_labourerdata labourer = new tbl_labourerdata();
            labourer.fld_address = package.fld_address;
            labourer.fld_dateofbirth = package.fld_dateofbirth.ToString();
            labourer.fld_email = package.fld_email;
            labourer.fld_firstname = package.fld_email;
            labourer.fld_gender = package.fld_gender;
            labourer.fld_labourerid = package.fld_labourerid;
            labourer.fld_lastname = package.fld_lastname;
            labourer.fld_phonenumber = package.fld_phonenumber;
            labourer.fld_zipcode = package.fld_zipcode;

            return View("Details", labourer);
        }
    }
}