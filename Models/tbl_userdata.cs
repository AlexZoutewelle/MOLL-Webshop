using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    [ElasticsearchType(Name = "User", IdProperty = nameof(fld_userid))]
    public class tbl_userdata
    {
        public int fld_userid { get; set; }
        public string fld_username { get; set; }
        public string fld_password { get; set; }
        public string fld_firstname { get; set; }
        public string fld_lastname { get; set; }
        public string fld_gender { get; set; }
        public string fld_address { get; set; }
        public string fld_zipcode { get; set; }
        public string fld_dateofbirth { get; set; }
        public string fld_phonenumber { get; set; }
        public string fld_email { get; set; }
        public string fld_activationcode { get; set; }
        public int fld_isactivated { get; set; }
        public string fld_adminPriv { get; set; }
    }
}
