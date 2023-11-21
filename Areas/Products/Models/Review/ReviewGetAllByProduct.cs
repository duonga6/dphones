namespace App.Areas.Products.Models.Review
{
    public class ReviewGetAllByProduct
    {
        public class ReviewResponse
        {
            public string UserId { set; get; } = null!;
            public string UserName { set; get; } = null!;
            public string? Image { set; get; }
            public string Content { set; get; } = null!;
            public int Rate { set; get; }
            public string DateCreated {set;get;} = null!;
        }
        public double AverageRate { set; get; }
        public List<ReviewResponse>? Reviews { set; get; }
    }
}