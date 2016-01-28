using System.Web;
using System.Web.Optimization;

namespace Granikos.Hydra.WebClient
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
                .Include("~/Scripts/FontDetector.js")
            );

            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/dist")
                .IncludeDirectory("~/Content/dist/", "*.css", true));

            bundles.Add(new ScriptBundle("~/Scripts/dist")
                .Include("~/Scripts/lib/jquery.js")
                .Include("~/Scripts/lib/angular.js")
                .Include("~/Scripts/lib/Chart.js")
                .Include("~/Scripts/lib/bootstrap.js")
                .IncludeDirectory("~/Scripts/lib/", "*.js", true));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
