using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using JG.Core;

namespace JahanGostar.Controllers
{
    [Authorize]
    public class RootController : Controller
    {
        protected UserIdentityClaims CurrentUser => new UserIdentityClaims(User as ClaimsPrincipal);

    }

    public class UserIdentityClaims : ClaimsPrincipal
    {
        public UserIdentityClaims(ClaimsPrincipal principal) : base(principal)
        {
        }
        public int StudentId => int.Parse(this.FindFirst(x => x.Type.ToLower() == "studentid").Value);
        public int GroupId => int.Parse(this.FindFirst(x => x.Type.ToLower() == "groupid").Value);
        public string Gender => this.FindFirst(x => x.Type.ToLower() == "gender").Value;
        public string ExpireDate => this.FindFirst(x => x.Type.ToLower() == "expiredate").Value;
        public string Username => this.FindFirst(x => x.Type.ToLower() == "username").Value;
        public string Personeli => this.FindFirst(x => x.Type.ToLower() == "personneli").Value;
        public string FirstName => this.Claims.FirstOrDefault(x => x.Type.ToLower() == "given_name")?.Value;
        public string LastName => this.Claims.FirstOrDefault(x => x.Type.ToLower() == "family_name")?.Value;
        public int Uc => int.Parse(this.FindFirst(x => x.Type.ToLower() == "uc").Value);
        public long Stid => long.Parse(this.FindFirst(x => x.Type.ToLower() == "stid").Value);
        public Role Role => (Role)int.Parse(this.FindFirst(x => x.Type.ToLower() == "role").Value);
        public int RoleCode => int.Parse(this.FindFirst(x => x.Type.ToLower() == "role").Value);
        public bool ISAdmin => bool.Parse(this.FindFirst(x => x.Type.ToLower() == "isadmin").Value);
    }
}