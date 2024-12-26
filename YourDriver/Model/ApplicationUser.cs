using Microsoft.AspNetCore.Identity;
using System.Data.Common;
using YourDriver.Model.AuthMangment;

namespace YourDriver.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<RefreshToken>? refreshtokens { get; set; }
    }
}
