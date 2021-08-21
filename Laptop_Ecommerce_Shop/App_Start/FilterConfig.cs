using System.Web;
using System.Web.Mvc;

namespace Laptop_Ecommerce_Shop
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
