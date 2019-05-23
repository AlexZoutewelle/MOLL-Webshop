using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models.Orders
{
    public class OrderHistory
    {
        int fld_OrderId { get; set; }
        DateTime fld_dateOrdered { get; set; }
        DateTime fld_targetDeliveryDate { get; set; }
        DateTime fld_dateDelivered { get; set; }
        DateTime fld_dateUpdated { get; set; }
    }
}