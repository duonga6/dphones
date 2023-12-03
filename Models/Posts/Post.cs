using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace App.Models.Posts
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Tiêu đề")]
        public string Title { set; get; } = string.Empty;
        [Required]
        [Display(Name = "Nội dung")]
        public string Content { set; get; } = string.Empty;
        [Display(Name = "Url")]
        public string? Slug { set; get; } = string.Empty;
        public string? Image { set; get; }
        public DateTime CreatedAt { set; get; }
        public DateTime UpdatedAt { set; get; }
    }
}