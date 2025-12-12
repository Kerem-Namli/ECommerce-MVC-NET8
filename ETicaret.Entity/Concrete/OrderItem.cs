using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Entity.Concrete
{
    public class OrderItem : BaseEntity
    {
        public int Quantity { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        // Foreign Keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        
        // Navigation Properties
        public virtual Order Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
