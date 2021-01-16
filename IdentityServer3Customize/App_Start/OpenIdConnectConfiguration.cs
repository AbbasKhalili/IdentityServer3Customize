using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;
using Owin;
using IdentityServer3.Core;
using Microsoft.Owin.Security;
using System.Linq;
using System.Web.Helpers;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security.OpenIdConnect;

namespace IdentityServer3Customize
{
    public static class OpenIdConnectConfiguration
    {
        public static void Config(IAppBuilder app, string identityServerUrl)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            AntiForgeryConfig.UniqueClaimTypeIdentifier =IdentityModel.JwtClaimTypes.Name;
            
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
               
                Authority = identityServerUrl + "/identity",
                AuthenticationType = "oidc",
                ClientId = "ClientId",
                ClientSecret = "ClientSecret",
                Scope = "openid profile email roles WebAPI",
                ResponseType = "id_token token",
                RedirectUri = identityServerUrl,
                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = false,
                //CallbackPath = new PathString(identityServerUrl + "/identity"),
                //CallbackPath = new PathString("/callback/"),
                //CallbackPath = new PathString("/Home"),
                ProtocolValidator = new OpenIdConnectProtocolValidator
                {
                    RequireNonce = true
                },
                
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = async n =>
                    {
                        var nid = new ClaimsIdentity(
                            n.AuthenticationTicket.Identity.AuthenticationType,
                            Constants.ClaimTypes.GivenName,
                            Constants.ClaimTypes.Role);

                        // get userinfo data
                        var endpoint = n.Options.Authority + "/connect/userinfo";
                        var userInfoClient = new UserInfoClient(endpoint);

                        var userInfo = await userInfoClient.GetAsync(n.ProtocolMessage.AccessToken);
                        userInfo.Claims.ToList().ForEach(ui => nid.AddClaim(new Claim(ui.Type, ui.Value)));

                        nid.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        nid.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));
                        nid.AddClaim(new Claim("expires_at", DateTimeOffset.Now.AddSeconds(int.Parse(n.ProtocolMessage.ExpiresIn)).ToString()));

                        n.AuthenticationTicket = new AuthenticationTicket(nid, n.AuthenticationTicket.Properties);
                    },

                    RedirectToIdentityProvider = context =>
                    {
                        var appBaseUrl = context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase;
                        var queryString = context.Request.Uri.PathAndQuery;
                        context.ProtocolMessage.RedirectUri = appBaseUrl + queryString;
                        
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.AuthenticationRequest)
                        {
                            context.ProtocolMessage.RedirectUri = identityServerUrl;
                            return Task.FromResult(0);
                        }

                        if (context.ProtocolMessage.RequestType != OpenIdConnectRequestType.LogoutRequest)
                            return Task.FromResult(0);
                        var idTokenHint = context.OwinContext.Authentication.User.FindFirst("id_token");

                        if (idTokenHint != null)
                            context.ProtocolMessage.IdTokenHint = idTokenHint.Value;

                        return Task.FromResult(0);
                    },

                    AuthenticationFailed = context =>
                    {
                        if (context.Exception is OpenIdConnectProtocolInvalidNonceException)
                        {
                            context.SkipToNextMiddleware();
                            return Task.FromResult(0);
                        }

                        if (!context.OwinContext.Authentication.User.Identity.IsAuthenticated)
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/");
                            return Task.FromResult(0);
                        }
                        context.HandleResponse();
                        context.Response.Redirect("/ValidatorError?message=" + context.Exception.Message);
                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}