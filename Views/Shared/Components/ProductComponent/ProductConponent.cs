using App.Models.Products;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class ProductComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Product model)
        {
            return View(model);
        }
    }
}