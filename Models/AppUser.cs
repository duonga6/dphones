using System.ComponentModel.DataAnnotations;
using App.Models.Chats;
using App.Models.Products;
using Microsoft.AspNetCore.Identity;

namespace App.Models
{
    public class AppUser : IdentityUser
    {
        public string? HomeAddress { set; get; }

        [Required]
        public required string FullName { set; get; }

        public string? UserAvatar { set; get; }

        public DateTime BirthDate { set; get; }

        public List<Order> Orders { set; get; } = new();
        public List<Review> Reviews { set; get; } = new();
        public List<Message> SentMessage { set; get; } = new();
        public List<Message> ReceivedMessage { set; get; } = new();
        public List<OrderStatus> OrderStatuses { set; get; } = new();
    }
}