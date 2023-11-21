using App.Areas.Products.Models;
using App.Models.Products;

namespace App.Models 
{
    public enum CategoryType{
        PC,
        Tablet
    }
    public class HomeCategory
    {
        public List<Brand> Brands {set;get;} = new();
        public List<Category> Categories {set;get;} = new();
        public List<ProductWithRate> Products {set;get;} = new();
        public List<int> PriceLevels = new();
        public CategoryType Type {set;get;} = CategoryType.PC;
    }
}