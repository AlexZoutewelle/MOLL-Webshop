using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalPayment.PayPal
{
    public class PayPalService
    {
        public static PayPalConfig getPayPalConfig()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            return new PayPalConfig()
            {
                AuthToken = configuration["PayPal:AuthToken"],
                Business = configuration["PayPal:Business"],
                PostUrl = "https://www.sandbox.paypal.com/cgi-bin-webscr",
                ReturnUrl = "https://www.sandbox.paypal.com/cgi-bin-webscr"
            };
        }
    }
}
