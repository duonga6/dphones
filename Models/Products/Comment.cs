using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Models.Products {
    public class Comment {
        [Key]
        public int Id {set;get;}
        [Required]
        [MaxLength(255)]
        public string Content {set;get;} = null!;
        public string? UserId {set;get;}
        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public AppUser? User {set;get;}
        [Required]
        [Range(1, 5)]
        public int Rate {set;get;}
        public int ProductId {set;get;}
        [ForeignKey(nameof(ProductId))]
        public Product Product {set;get;} = null!;
    }
}