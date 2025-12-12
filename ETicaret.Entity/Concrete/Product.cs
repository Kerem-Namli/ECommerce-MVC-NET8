using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Entity.Concrete
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int Stock { get; set; }
        
        public bool IsFeatured { get; set; }
        
        // Foreign Key
        public int CategoryId { get; set; }
        
        // Navigation Property
        public virtual Category Category { get; set; } = null!;
        
        // Navigation Property for OrderItems
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        // Navigation Property for CartItems
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
