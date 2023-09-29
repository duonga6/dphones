using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class AppUser : IdentityUser
    {
        public string? HomeAddress {set;get;}

        [Required]
        public required string FullName {set;get;}

        public string? UserAvatar {set;get;}

        public DateTime BirthDate {set;get;}
    }
}