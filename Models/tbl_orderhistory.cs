using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class tbl_orderhistory
    {
        public int fld_orderid { get; set; }
        public string fld_orderstatus { get; set; }
        public string fld_lastAction { get; set; }
        public DateTime fld_ActionDate { get; set; }
    }
}
