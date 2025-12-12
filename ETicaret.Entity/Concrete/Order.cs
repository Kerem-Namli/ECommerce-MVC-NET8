using System.ComponentModel.DataAnnotations.Schema;

namespace ETicaret.Entity.Concrete
{
    public enum OrderStatus
    {
        Pending = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4
    }
    
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        // Shipping Information
        public string ShippingAddress { get; set; } = null!;
        public string ShippingCity { get; set; } = null!;
        public string ShippingDistrict { get; set; } = null!;
        public string? ShippingPostalCode { get; set; }
        public string ShippingPhone { get; set; } = null!;
        
        // Payment Information
        public string PaymentMethod { get; set; } = null!;
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        
        // Foreign Key
        public string UserId { get; set; } = null!;
        
        // Navigation Property
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
