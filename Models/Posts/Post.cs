using System.ComponentModel.DataAnnotations;

namespace App.Models.Posts
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Title { set; get; } = string.Empty;
        [Required]
        public string Content { set; get; } = string.Empty;
        public DateTime CreatedAt { set; get; }
        public DateTime UpdatedAt { set; get; }
    }
}