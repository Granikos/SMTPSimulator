using System.Web;
using System.Web.Mvc;

namespace Granikos.SMTPSimulator.WebClient
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
