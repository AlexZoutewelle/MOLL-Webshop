using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Nest;

namespace TestWebApp.Models
{
    [ElasticsearchType(Name = "Services", IdProperty = nameof(fld_serviceid))]
    public class tbl_servicedata
    {
        public tbl_servicedata() { }

        public int fld_serviceid { get; set; }
        public string fld_name { get; set; }
        public string fld_description { get; set; }
        public string fld_category { get; set; }
        public string fld_imagelink { get; set; }

    }
}
