using System.Security.Claims;
using App.Models;
using Microsoft.AspNetCore.Identity;

namespace App.Areas.Identity.Models.User
{
    public class UserIndexModel
    {
        public int TotalUser {set;get;}

        public List<UserModel>? User {set;get;}
    }

    public class UserModel : AppUser
    {
        public List<string>? Roles {set;get;}
    }
}