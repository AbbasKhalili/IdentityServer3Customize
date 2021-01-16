using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;

namespace IdentityServer3Customize.IdentityServer
{
    public class JetonUserService : UserServiceBase
    {
        private readonly UserRepository _userRepository;
        private readonly OwinEnvironmentService _owinEnv;

        public JetonUserService(OwinEnvironmentService env)
        {
            _userRepository = new UserRepository();
            _owinEnv = env;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.Subject.Identity.IsAuthenticated)
            {
                var identity = new ClaimsIdentity();
                context.AllClaimsRequested = true;

                identity.AddClaims(new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "FirstName"),
                    new Claim(Constants.ClaimTypes.FamilyName, "LastName")
                });
                context.IssuedClaims = identity.Claims;
            }
            await Task.FromResult(context);
        }

        public override Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }
        
        public static Dictionary<string, int> CaptchaStorage = new Dictionary<string, int>();
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {

            var clientName = GetClientName(context.SignInMessage.ClientId);
            var requestContext = (System.Web.Routing.RequestContext)_owinEnv.Environment["System.Web.Routing.RequestContext"];

            
            var enteredCaptcha = 0;
            int.TryParse(requestContext.HttpContext.Request.Form["Captcha"], out enteredCaptcha);

            var requestId = requestContext.HttpContext.Request.Params["signin"];
            var serverCaptcha = CaptchaStorage.FirstOrDefault(a => a.Key == requestId).Value;
            if (requestId != null) CaptchaStorage.Remove(requestId);

            if (serverCaptcha != enteredCaptcha)
            {
                context.AuthenticateResult = new AuthenticateResult("کد امنیتی اشتباه است.");
                return;
            }

            var query = await _userRepository.GetUsersByUserNamePasswordAsync(context.UserName, context.Password);

            if (query == null) return;

            if (query.Enabled && query.Username != null)
            {
                var authenticateResult = GetAuthenticateResult(query);
                context.AuthenticateResult = authenticateResult;
            }
            else
            {
                context.AuthenticateResult = new AuthenticateResult(query.Subject);
            }
        }

        private string GetClientName(string clientid)
        {
            var clientlist = Clients.Get();
            return clientlist.FirstOrDefault(x => x.ClientId == clientid)?.ClientName;
        }

        private static AuthenticateResult GetAuthenticateResult(InMemoryUser user)
        {
            var authenticateResult = new AuthenticateResult(user.Subject,
                user.Username,
                user.Claims,
                Constants.BuiltInIdentityProvider,
                Constants.AuthenticationMethods.Password);

            return authenticateResult;
        }
    }
}