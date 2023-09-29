using App.Models;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.User
{
    public class AddRoleUserModel
    {
        public required AppUser User {set;get;}

        public List<string>? RolesName {set;get;}

        public List<IdentityRoleClaim<string>>? ClaimsInRole {set;get;}

        public List<IdentityUserClaim<string>>? ClaimsInUser {set;get;}
    }
}