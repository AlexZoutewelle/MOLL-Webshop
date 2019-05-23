using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;


namespace TestWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public const string SessionKeyName = "Test";

        public void OnGet()
        {
            HttpContext.Session.SetString(SessionKeyName, "Hey hallo");
        }
    }
}
