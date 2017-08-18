// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Web;
using System.Web.Optimization;

namespace Granikos.SMTPSimulator.WebClient
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
