using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;

namespace IdentityServer3Customize.IdentityServer
{
    public class UserRepository 
    {
        public async Task<InMemoryUser> GetUsersByUserNamePasswordAsync(string username, string password)
        {
            var identity = new InMemoryUser {Enabled = false, Username = "UserName", Password = "password"};

            identity.Enabled = true;
            identity.Subject = "username";
            identity.Claims = new[] { new Claim(Constants.ClaimTypes.Role, "admin") };
            return identity;
        }
    }
}