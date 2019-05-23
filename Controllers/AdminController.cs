using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestWebApp.ElasticSearch;
using TestWebApp.ElasticSearch.Queries;
using TestWebApp.Models;
using TestWebApp.Models.ProductPackage;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestWebApp.Controllers
{
    public class AdminController : Controller
    {
        // GET: /<controller>/
        public IActionResult Dashboard()
        {
            return View("Index");
        }

        public IActionResult ManageUsers()
        {
            List<tbl_userdata> allUsers = EsUserQuery.AllUsers();
            return View(allUsers);
        }

        public IActionResult ManageServices()
        {
            List<tbl_servicedata> allUsers = EsServiceQuery.AllServices();
            return View(allUsers);
        }

        public IActionResult ManageLabourers()
        {
            List<tbl_labourerdata> allUsers = EsLabourerQuery.AllLabourers();
            return View(allUsers);
        }

        public IActionResult ManageOLS()
        {
            List<OfferedLabourerService> allOLS = EsOLSQuery<OfferedLabourerService>.getAll();
            return View(allOLS);
        }

        public int CreateItem(string insertedDic, string type)
        {
            //return the id back to the JS Datatable!
            switch (type)
            {
                case "tbl_userdata":
                    tbl_userdata newUser = JsonConvert.DeserializeObject<tbl_userdata>(insertedDic);

                    //Check if the email has been taken already
                    int emailIsTaken = MollShopContext.CheckIfUserExists(newUser.fld_email);

                    if (emailIsTaken == 0)
                    {
                        //Email has not yet been taken

                        //Salt and Hash the password
                        newUser.fld_password = MollShopContext.SaltNHash(newUser.fld_password);

                        newUser.fld_userid = MollShopContext.CreateRow(newUser, type);

                        if(newUser.fld_dateofbirth == "")
                        {
                            newUser.fld_dateofbirth = null;
                        }
                        EsUpdater<tbl_userdata>.InsertDocument(newUser, "moll_users", "User", newUser.fld_userid.ToString());
                        return newUser.fld_userid;
                    }

                    else
                    {
                        //Email has been taken
                        return -1;
                    }

                case "tbl_servicedata":
                    tbl_servicedata newService = JsonConvert.DeserializeObject<tbl_servicedata>(insertedDic);
                    newService.fld_serviceid = MollShopContext.CreateRow(newService, type);
                    EsUpdater<tbl_servicedata>.InsertDocument(newService, "moll_dataservices", "Services", newService.fld_serviceid.ToString());

                    return newService.fld_serviceid;

                case "tbl_labourerdata":
                    tbl_labourerdata newLabourer = JsonConvert.DeserializeObject<tbl_labourerdata>(insertedDic);
                    newLabourer.fld_labourerid = MollShopContext.CreateRow(newLabourer, type);
                    EsUpdater<tbl_labourerdata>.InsertDocument(newLabourer, "moll_labourers", "Labourer", newLabourer.fld_labourerid.ToString());

                    return newLabourer.fld_labourerid;

                default:
                    break;
            }

            return 0;
        }

        public int EditItem(string insertedDic, string type)
        {
            switch (type)
            {
                case "tbl_userdata":
                    try
                    {
                        tbl_userdata currentUser = JsonConvert.DeserializeObject<tbl_userdata>(insertedDic);

                        //Dates and ElasticSearch do not mix very well, so we do a little check beforehand
                        if (currentUser.fld_dateofbirth == "")
                        {
                            currentUser.fld_dateofbirth = null;
                        }

                        EsUpdater<tbl_userdata>.UpsertDocument(currentUser, "moll_users", "User", currentUser.fld_userid);
                        MollShopContext.UpdateRow(currentUser, "fld_UserId", currentUser.fld_userid);
                    }
                    catch(Exception e)
                    {
                        return -1;
                    }

                    break;

                case "tbl_servicedata":
                    tbl_servicedata currentService = JsonConvert.DeserializeObject<tbl_servicedata>(insertedDic);

                    //Update the stand-alone service document
                    EsUpdater<tbl_servicedata>.UpsertDocument(currentService, "moll_dataservices", "Services", currentService.fld_serviceid);

                    //Find all OLS documents in ES that contain this service
                    List<OfferedLabourerService> packages = EsOLSQuery<OfferedLabourerService>.getByService(currentService.fld_serviceid);

                    //Foreach OLS ID, update it with the current service
                    foreach (OfferedLabourerService package in packages)
                    {
                        package.fld_name = currentService.fld_name;
                        package.fld_category = currentService.fld_category;
                        package.fld_description = currentService.fld_description;
                        package.fld_imagelink = currentService.fld_imagelink;

                        EsUpdater<OfferedLabourerService>.UpsertDocument(package, "moll_ols", "OLS", package.fld_offeredserviceid);
                    }

                    MollShopContext.UpdateRow(currentService, "fld_ServiceId", currentService.fld_serviceid);

                    break;

                case "tbl_labourerdata":
                    tbl_labourerdata currentLabourer = JsonConvert.DeserializeObject<tbl_labourerdata>(insertedDic);

                    //Update the stand-alone labourer document
                    EsUpdater<tbl_labourerdata>.UpsertDocument(currentLabourer, "moll_labourers", "Labourer", currentLabourer.fld_labourerid);

                    //Find all OLS documents in ES that contain this labourer
                    List<OfferedLabourerService> olspackages = EsOLSQuery<OfferedLabourerService>.getByLabourer(currentLabourer.fld_labourerid);

                    //Foreach OLS Id, update it with the current labourer
                    foreach (OfferedLabourerService package in olspackages)
                    {
                        package.fld_address = currentLabourer.fld_address;
                        package.fld_firstname = currentLabourer.fld_firstname;
                        package.fld_email = currentLabourer.fld_email;
                        package.fld_gender = currentLabourer.fld_gender;
                        package.fld_lastname = currentLabourer.fld_lastname;
                        package.fld_phonenumber = currentLabourer.fld_phonenumber;
                        package.fld_zipcode = currentLabourer.fld_zipcode;

                        EsUpdater<OfferedLabourerService>.UpsertDocument(package, "moll_ols", "OLS", package.fld_offeredserviceid);
                    }

                    MollShopContext.UpdateRow(currentLabourer, "fld_LabourerId", currentLabourer.fld_labourerid);

                    break;
                default:
                    break;
            }

            return 1;
        }

        public IActionResult DeleteItem(int id, string type)
        {
            switch (type)
            {
                case "tbl_userdata":
                    MollShopContext.DeleteRow(type, "fld_UserId", id);
                    EsUpdater<tbl_userdata>.DeleteDocument(id, "User", "moll_users");

                    break;
                case "tbl_servicedata":

                    MollShopContext.DeleteRow(type, "fld_serviceid", id);
                    EsUpdater<tbl_servicedata>.DeleteDocument(id, "Services", "moll_dataservices");

                    break;
                case "tbl_labourerdata":
                    MollShopContext.DeleteRow(type, "fld_labourerid", id);
                    EsUpdater<tbl_labourerdata>.DeleteDocument(id, "Labourer", "moll_labourers");
                    //Delete the OLS for this labourer as well

                    break;
                case "offeredlabourerservice":
                    MollShopContext.DeleteRow("tbl_offeredservicesdata", "fld_offeredServiceId", id);
                    EsUpdater<OfferedLabourerService>.DeleteDocument(id, "OLS", "moll_ols");
                    break;
                default:
                    break;
            }

            return Ok();

        }


        //This action checks for dependencies. For instance, if a service is currently in an OfferedLabourerService.
        public int checkForDependency (int id, string type)
        {
            switch (type)
            {
                case "tbl_servicedata":
                    List<OfferedLabourerService> packages = EsOLSQuery<tbl_servicedata>.getByService(id);

                    return packages.Count;

                case "tbl_labourerdata":
                    List<OfferedLabourerService> Olspackages = EsOLSQuery<tbl_servicedata>.getByLabourer(id);

                    return Olspackages.Count;
                default:
                    return 0;
            }
        }




        public IActionResult ItemPreview(string insertedDic, string type)
        {
 

            switch (type)
            {
                case "tbl_servicedata":
                    tbl_servicedata currentService = JsonConvert.DeserializeObject<tbl_servicedata>(insertedDic);
                    return PartialView("ServicePreview", currentService);

                case "tbl_labourerdata":
                    tbl_labourerdata currentLabourer = JsonConvert.DeserializeObject<tbl_labourerdata>(insertedDic);
                    return PartialView("LabourerPreview", currentLabourer);

                default:
                    break;
            }

            return NotFound();
        }


        public OLS CreateOLS(string offeredService, string serviceId, string labourerId)
        {
            tbl_servicedata service = EsServiceQuery.FindById(Convert.ToInt32(serviceId));
            tbl_labourerdata labourer = EsLabourerQuery.FindById(Convert.ToInt32(labourerId));
            tbl_offeredservicesdata offeredServiceObj = JsonConvert.DeserializeObject<tbl_offeredservicesdata>(offeredService);
            offeredServiceObj.fld_labourerid = labourer.fld_labourerid;
            offeredServiceObj.fld_serviceid = service.fld_serviceid;

            offeredServiceObj.fld_offeredserviceid = MollShopContext.CreateRow(offeredServiceObj, "tbl_offeredservicesdata");

            OLS ols = ConstructOLS(service, labourer, offeredServiceObj);

            //Because ElasticSearch does not support decimal numbers, we must multiply the cost by a 100
            ols.fld_cost = ols.fld_cost * 100;

            EsUpdater<OLS>.InsertDocument(ols, "moll_ols", "OLS", ols.fld_offeredserviceid.ToString());

            //return the OfferedLabourerService, so we can later render it into the Datatable on the ManageOLS page
            //We must divide the cost by a 100 again, to render it correctly

            ols.fld_cost = ols.fld_cost / 100;
            return ols;
        }




        public IActionResult OLSPreview(string offeredService, string serviceId, string labourerId)
        {
            tbl_servicedata service = EsServiceQuery.FindById(Convert.ToInt32(serviceId));
            tbl_labourerdata labourer = EsLabourerQuery.FindById(Convert.ToInt32(labourerId));
            tbl_offeredservicesdata offeredServiceObj = JsonConvert.DeserializeObject<tbl_offeredservicesdata>(offeredService);
            offeredServiceObj.fld_offeredserviceid = 0;

            OLS ols = ConstructOLS(service, labourer, offeredServiceObj);


            return PartialView("OLSPreview", ols);
        }

        public OLS ConstructOLS(tbl_servicedata service, tbl_labourerdata labourer, tbl_offeredservicesdata offeredServiceObj)
        {
            OLS ols = new OLS();
            ols.fld_addedtowishlist = 0;
            ols.fld_address = labourer.fld_address;
            ols.fld_area = offeredServiceObj.fld_area;
            ols.fld_category = service.fld_category;
            ols.fld_cost = Convert.ToInt32(offeredServiceObj.fld_cost * 100);
            ols.fld_dateofbirth = labourer.fld_dateofbirth;
            ols.fld_description = service.fld_description;
            ols.fld_email = labourer.fld_email;
            ols.fld_firstname = labourer.fld_firstname;
            ols.fld_gender = labourer.fld_gender;
            ols.fld_imagelink = service.fld_imagelink;
            ols.fld_labourerid = labourer.fld_labourerid;
            ols.fld_lastname = labourer.fld_lastname;
            ols.fld_name = service.fld_name;
            ols.fld_phonenumber = labourer.fld_phonenumber;
            ols.fld_serviceid = service.fld_serviceid;
            ols.fld_stillavailable = offeredServiceObj.fld_stillavailable;
            ols.fld_timefirst = offeredServiceObj.fld_timefirst.ToString();
            ols.fld_timelast = offeredServiceObj.fld_timelast.ToString();
            ols.fld_timesbought = offeredServiceObj.fld_timesbought;
            ols.fld_zipcode = labourer.fld_zipcode;

            ols.fld_offeredserviceid = offeredServiceObj.fld_offeredserviceid;

            return ols;
        }


        public IActionResult OLSCreationDiv()
        {
            return PartialView("OlsCreation");
        }

        public OfferedLabourerService UpdateOLS(string insertedDic)
        {
            tbl_offeredservicesdata currentOffer = JsonConvert.DeserializeObject<tbl_offeredservicesdata>(insertedDic);

            MollShopContext.UpdateRow(currentOffer, "fld_offeredserviceid", currentOffer.fld_offeredserviceid);

            //Because ElasticSearch does not support decimal numbers, we must multiply the cost by a 100
            currentOffer.fld_cost = currentOffer.fld_cost * 100;

            OfferedLabourerService currentOLS = EsOLSQuery<OfferedLabourerService>.findByOfferedServiceId(currentOffer.fld_offeredserviceid);
            currentOLS.fld_cost = currentOffer.fld_cost;
            currentOLS.fld_area = currentOffer.fld_area;
            currentOLS.fld_timefirst = currentOffer.fld_timefirst;
            currentOLS.fld_timelast = currentOffer.fld_timelast;
            currentOLS.fld_stillavailable = currentOffer.fld_stillavailable;



            EsUpdater<OfferedLabourerService>.UpsertDocument(currentOLS, "moll_ols", "OLS", currentOLS.fld_offeredserviceid);

            //To render it correctly in the datatable, we divice the cost by 100 again
            currentOLS.fld_cost = currentOLS.fld_cost / 100;

            return currentOLS;
        }



    }
}
