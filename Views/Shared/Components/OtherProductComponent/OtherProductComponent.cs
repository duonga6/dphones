using App.Models.Products;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class OtherProductComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<Product> model)
        {
            return View(model);
        }
    }
}