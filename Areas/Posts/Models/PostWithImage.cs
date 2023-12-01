using System.ComponentModel.DataAnnotations;

namespace App.Areas.Posts.Models
{
    public class PostWithImage : App.Models.Posts.Post
    {
        public IFormFile? ImageFile { set; get; }
    }
}