using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class OfferedService
    {
        public int fld_offeredserviceid { get; set; }
        public int fld_serviceid { get; set; }
        public int fld_labourerid { get; set; }
        public int fld_addedtowishlist { get; set; }
        public int fld_timesbought { get; set; }
        public double fld_cost { get; set; }
        public TimeSpan fld_timefirst { get; set; }
        public TimeSpan fld_timelast { get; set; }
        public string fld_area { get; set; }
        public char fld_stillavailable { get; set; }

        public int lowerCost { get; set; }
        public int higherCost { get; set; }
        public int timeAvailableBegin { get; set; }
        public int timeAvailableEnd { get; set; }

        
    }
}
