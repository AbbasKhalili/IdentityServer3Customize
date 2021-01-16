using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using IdentityServer3.AccessTokenValidation;
using Owin;

namespace IdentityServer3Customize
{
    public static class WebApiConfig
    {
        public static void Register(IAppBuilder app, string identityServerUrl, HttpConfiguration config)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            
            app.Map("/api", inner =>
            {
                var bearerTokenOptions = new IdentityServerBearerTokenAuthenticationOptions
                {
                    Authority = identityServerUrl + "/identity",
                    RequiredScopes = new[] { "WebAPI" },
                    DelayLoadMetadata = true,
                };

                AntiForgeryConfig.UniqueClaimTypeIdentifier = "StudentId";

                inner.UseIdentityServerBearerTokenAuthentication(bearerTokenOptions);
                config.MapHttpAttributeRoutes();
                
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "v0/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                inner.UseWebApi(config);
            });

            app.Map("/api", inner =>
            {
                //config.MapHttpAttributeRoutes();
                config.Routes.MapHttpRoute(
                    name: "DevApi",
                    routeTemplate: "dev/v0/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                inner.UseWebApi(config);
            });
        }
    }
}
