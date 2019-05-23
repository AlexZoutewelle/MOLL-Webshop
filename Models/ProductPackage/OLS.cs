using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models.ProductPackage
{
    [ElasticsearchType(Name = "OLS", IdProperty = nameof(fld_offeredserviceid))]
    public class OLS
    {
        public int fld_offeredserviceid { get; set; }
        public int fld_addedtowishlist { get; set; }
        public int fld_timesbought { get; set; }
        public int fld_cost { get; set; }
        public string fld_timefirst { get; set; }
        public string fld_timelast { get; set; }
        public string fld_area { get; set; }
        public char fld_stillavailable { get; set; }

        public int fld_labourerid { get; set; }
        public string fld_firstname { get; set; }
        public string fld_lastname { get; set; }
        public string fld_gender { get; set; }
        public string fld_address { get; set; }
        public string fld_zipcode { get; set; }
        public string fld_dateofbirth { get; set; }
        public string fld_phonenumber { get; set; }
        public string fld_email { get; set; }

        public int fld_serviceid { get; set; }
        public string fld_name { get; set; }
        public string fld_category { get; set; }
        public string fld_imagelink { get; set; }
        public string fld_description { get; set; }



    }
}
