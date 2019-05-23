using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class tbl_orderstatus
    {
        public int fld_orderid { get; set; }
        public DateTime fld_dateOrdered { get; set; }
        public DateTime fld_targetDeliveryDate { get; set; }
        public DateTime fld_dateDelivered { get; set; }
        public DateTime fld_DateUpdated { get; set; }
    }
}
