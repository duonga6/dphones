using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.Roles
{
    public class RoleModel : IdentityRole
    {
        public List<Claim>? Claims {set;get;}
    }
}