using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Models
{
    public class OLSSearchModel
    {
        public string fld_firstname { get; set; }
        public string fld_lastname { get; set; }
        public string fld_gender { get; set; }

        public string fld_name { get; set; }
        public string fld_category { get; set; }

        public TimeSpan fld_timefirst { get; set; }
        public TimeSpan fld_timelast { get; set; }
        public double fld_timefirstinsec { get; set; }
        public double fld_timelastinsec { get; set; }

        public string fld_area { get; set; }
        public char fld_stillavailable { get; set; }

        public int lowerCost { get; set; }
        public int higherCost { get; set; }
        public int fld_cost { get; set; }


    }
}
