using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Entity.Concrete
{
    public class CartItem : BaseEntity
    {
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        // Foreign Keys
        public int CartId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation Properties
        public virtual Cart Cart { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
