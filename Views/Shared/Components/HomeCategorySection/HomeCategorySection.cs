using App.Models;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class HomeCategorySection : ViewComponent
    {
        public IViewComponentResult Invoke(HomeCategory model)
        {
            return View(model);
        }
    }
}