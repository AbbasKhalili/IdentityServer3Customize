using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using IdentityServer3Customize;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace IdentityServer3Customize
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var url = ConfigurationManager.AppSettings["IdentityServerURI"];
            
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            var httpconfig = new HttpConfiguration();

            IdentityServerConfiguration.Config(app);

            OpenIdConnectConfiguration.Config(app, url);
            
            WebApiConfig.Register(app, url, httpconfig);
            app.Use((context, next) =>
            {
                context.Response.Headers.Remove("Server");
                return next.Invoke();
            });
            app.UseStageMarker(PipelineStage.PostAcquireState);
            app.UseWebApi(httpconfig);

        }
    }
}