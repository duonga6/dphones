using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace App.Models.Chats
{
    public class Message
    {
        public int Id { set; get; }
        public string? SenderId { set; get; } = string.Empty;
        public string? ReceiverId { set; get; } = string.Empty;
        public bool FromAdmin { set; get; }
        [Required]
        public string Content { set; get; } = string.Empty;
        public bool Seen { set; get; }
        public DateTime CreatedAt { set; get; } = DateTime.Now;

        public AppUser Sender { set; get; } = null!;
        public AppUser Receiver { set; get; } = null!;


    }
}