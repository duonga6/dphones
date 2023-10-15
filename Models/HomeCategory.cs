using App.Models.Products;

namespace App.Models 
{
    public class HomeCategory
    {
        public List<Brand> Brands {set;get;} = new();
        public List<Category> Categories {set;get;} = new();
        public List<Product> Products {set;get;} = new();
    }
}