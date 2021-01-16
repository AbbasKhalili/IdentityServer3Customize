using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;

namespace IdentityServer3Customize.IdentityServer
{
    public class JetonViewService : IViewService
    {
        private readonly IClientStore _clientStore;
        public JetonViewService(IClientStore clientStore)
        {
            this._clientStore = clientStore;
        }

        public virtual async Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            var client = await _clientStore.FindClientByIdAsync(message.ClientId);
            var name = client != null ? client.ClientName : null;
            return await Render(model, "login", name);
        }

        public Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            return Render(model, "logout");//log out
        }

        public Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            return Render(model, "loggedOut");//loggedOut
        }


        public Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            return Render(model, "consent");
        }

        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            return Render(model, "permissions");
        }

        public virtual Task<Stream> Error(ErrorViewModel model)
        {
            return Render(model, "error");
        }

        protected virtual Task<Stream> Render(CommonViewModel model, string page, string clientName = null)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.None,
                new Newtonsoft.Json.JsonSerializerSettings()
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                });

            string html = LoadHtml(page);
            html = Replace(html, new
            {
                siteName = Microsoft.Security.Application.Encoder.HtmlEncode(model.SiteName),
                model = Microsoft.Security.Application.Encoder.HtmlEncode(json),
                clientName = clientName
            });

            return Task.FromResult(StringToStream(html));
        }

        private string LoadHtml(string name)
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"IdentityServer\Pages");
            file = Path.Combine(file, name + ".html");
            return File.ReadAllText(file);
        }

        string Replace(string value, IDictionary<string, object> values)
        {
            foreach (var key in values.Keys)
            {
                var val = values[key];
                val = val ?? String.Empty;
                if (val != null)
                {
                    value = value.Replace("{" + key + "}", val.ToString());
                }
            }
            return value;
        }

        string Replace(string value, object values)
        {
            return Replace(value, Map(values));
        }

        IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;

            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }

        Stream StringToStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(s);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}