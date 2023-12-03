using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace App.Models.Chats
{
    public class Message
    {
        public int Id { set; get; }
        [Required]
        public string Sender { set; get; } = string.Empty;
        [Required]
        public string Receiver { set; get; } = string.Empty;
        public string? AdminId { set; get; }
        [Required]
        public string Content { set; get; } = string.Empty;
        public bool Seen { set; get; }
        public DateTime CreatedAt { set; get; } = DateTime.Now;
    }
}