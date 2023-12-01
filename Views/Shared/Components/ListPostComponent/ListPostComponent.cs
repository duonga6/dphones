using App.Models.Posts;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class ListPostComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Post> model)
        {
            return View(model);
        }
    }
}