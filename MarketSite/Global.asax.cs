using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MarketSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
        }

        //protected void Application_BeginRequest(Object sender, EventArgs e)
        //{
        //    HttpCookie MyLang = Request.Cookies["MyLang"];
        //    if (MyLang != null)
        //    {
        //        //Thread.CurrentThread.CurrentCulture = new CultureInfo(MyLang.Value);
        //        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(MyLang.Value);
        //        Thread.CurrentThread.CurrentUICulture = new CultureInfo(MyLang.Value);
        //    }
        //}

    }
}
