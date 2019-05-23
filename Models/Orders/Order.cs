using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models.Orders
{
    public class Order
    {
        public int fld_OrderId { get; set; }
        public int fld_OfferedServiceId { get; set; }
        public string fld_email { get; set; }
    }
}
