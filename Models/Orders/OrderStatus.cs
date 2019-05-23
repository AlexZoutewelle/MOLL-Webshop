using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models.Orders
{
    public class OrderStatus
    {
        int fld_OrderId { get; set; }
        string fld_orderStatus { get; set; }
        string fld_LastAction { get; set; }
        DateTime fld_ActionDate { get; set; }
    }
}
