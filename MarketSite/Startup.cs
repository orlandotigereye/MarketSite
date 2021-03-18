using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace MarketSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                //識別的Cookie名稱
                //AuthenticationType = "ApplicationCookie",
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                //無權限時導頁
                //LoginPath = new PathString("/Home/index")
                LoginPath = new PathString("/Account/Login")
                //ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}

