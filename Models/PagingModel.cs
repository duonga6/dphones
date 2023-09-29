namespace App.Models
{
    public class PagingModel
    {
        public int CurrentPage {set;get;}
        public int CountPage {set;get;}
        public required Func<int?, string> GenerateUrl {set; get;}
    }
}