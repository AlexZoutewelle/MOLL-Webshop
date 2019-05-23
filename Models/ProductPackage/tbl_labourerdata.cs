using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    [ElasticsearchType(Name = "Labourer", IdProperty = nameof(fld_labourerid))]
    public class tbl_labourerdata
    {
        public int fld_labourerid { get; set; }
        public string fld_firstname { get; set; }
        public string fld_lastname { get; set; }
        public string fld_gender { get; set; }
        public string fld_address { get; set; }
        public string fld_zipcode { get; set; }
        public string fld_dateofbirth { get; set; }
        public string fld_phonenumber { get; set; }
        public string fld_email { get; set; }
    }
}
