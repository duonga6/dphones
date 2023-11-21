namespace App.Areas.Products.Models.Review
{
    public class ReviewCreate
    {
        public int ProductId {set;get;}
        public string Content {set;get;} = null!;
        public int Rate {set;get;}
    }
}