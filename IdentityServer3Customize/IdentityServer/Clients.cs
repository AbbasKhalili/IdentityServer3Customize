using System.Collections.Generic;
using IdentityServer3.Core.Models;

namespace IdentityServer3Customize.IdentityServer
{
    public static class Clients
    {
        public static IEnumerable<Client> Get()
        {
            var idsUrl = System.Configuration.ConfigurationManager.AppSettings["IdentityServerURI"];
            
            return new[]
            {
                new Client
                {
                    ClientName = "SPA Client",
                    ClientId = "ClientId",
                    ClientSecrets = new List<Secret> { new Secret("ClientSecret".Sha256()) },
                    Enabled = true,
                    Flow = Flows.Implicit,
                    RedirectUris = new List<string> { idsUrl },
                    AllowedScopes = new List<string> { "openid","profile","email","roles","WebAPI" },
                    RequireConsent = false,
                    AllowRememberConsent = false,
                    IdentityTokenLifetime = 3600 *2,
                    AccessTokenLifetime = 3600*2,
                }
            };
        }
    }
}