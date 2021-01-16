using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Helpers;
using IdentityServer3.Core;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3Customize.IdentityServer;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace IdentityServer3Customize
{
    public static class IdentityServerConfiguration
    {
        public static void Config(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Subject;
            
            app.Map("/identity", idsrvApp =>
            {
                var factory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Clients.Get())
                    .UseInMemoryScopes(Scopes.Get());

                factory.ViewService = new Registration<IViewService, JetonViewService>();

                factory.RedirectUriValidator = new Registration<IRedirectUriValidator, RedirectUriValidator>();
                factory.Register(new Registration<IUserRepository>(res => new UserRepository()));
                factory.UserService = new Registration<IUserService, JetonUserService>();

                idsrvApp.UseIdentityServer(new IdentityServerOptions
                {
                    SiteName = "IdentityServer3Customize",
                    SigningCertificate = LoadCertificate(),
                    Factory = factory,
                    RequireSsl = false,
                });
            });
            
            
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                CookieHttpOnly = true,
                //CookieSecure = CookieSecureOption.SameAsRequest,
                CookieName = "IdentityServer3Customize",
                //CookieManager = new SystemWebCookieManager(),
                ExpireTimeSpan = TimeSpan.FromHours(2)
            });
        }

        static X509Certificate2 LoadCertificate()
        {
            return new X509Certificate2($@"{AppDomain.CurrentDomain.BaseDirectory}\IdentityServer\Certificates\signcert.pfx", "111111");
        }
    }
}