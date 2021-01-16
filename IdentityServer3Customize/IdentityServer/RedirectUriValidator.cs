using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3Customize.IdentityServer
{
    public class RedirectUriValidator : IRedirectUriValidator
    {
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            var exists = client.RedirectUris.FirstOrDefault(a => a.GetHashCode() == requestedUri.GetHashCode());
            var result = exists != null;

            return Task.FromResult(result);
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            var exists = client.PostLogoutRedirectUris.FirstOrDefault(a => a.GetHashCode() == requestedUri.GetHashCode());
            var result = exists != null;
            return Task.FromResult(result);
        }
    }
}