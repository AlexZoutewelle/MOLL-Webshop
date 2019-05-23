using Microsoft.AspNetCore.Mvc;
using TestWebApp.Models.ProductPackage;

namespace TestWebApp.ViewComponents
{
    public class SimpleSearchBar : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("../SearchComponents/SimpleSearchBar", new OfferedLabourerService());
        }

    }
}
