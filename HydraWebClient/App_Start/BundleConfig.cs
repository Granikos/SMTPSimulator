using System.Web;
using System.Web.Optimization;

namespace HydraWebClient
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/Hydra")
                .Include("~/Scripts/helpers.js")
                .IncludeDirectory("~/Scripts/Controllers", "*.js")
                .Include("~/Scripts/HydraApp.js")
            );

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));
        }
    }
}
