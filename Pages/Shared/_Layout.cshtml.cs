using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestWebApp.Pages.Shared
{
    public class _Layout : PageModel
    {
        public void OnGet()
        {
            HttpContext.Session.SetString("LayoutTest", "Gelukt");
        }
    }
}
