using System.Web;
using System.Web.Optimization;

namespace Granikos.NikosTwo.WebClient
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/main.js")
                .Include("~/Scripts/helpers.js")
                .IncludeDirectory("~/Scripts/Controllers", "*.js")
                .Include("~/Scripts/app.js")
                .Include("~/Scripts/FontDetector.js")
            );

            bundles.Add(new StyleBundle("~/Content/main.css")
                .Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/dist/dist.css")
                .IncludeDirectory("~/Content/dist/", "*.css", true));

            bundles.Add(new ScriptBundle("~/Scripts/dist.js")
                .Include("~/Scripts/lib/jquery.js")
                .Include("~/Scripts/lib/angular.js")
                .Include("~/Scripts/lib/Chart.js")
                .Include("~/Scripts/lib/bootstrap.js")
                .IncludeDirectory("~/Scripts/lib/", "*.js", true));

            // BundleTable.EnableOptimizations = true;
        }
    }
}
