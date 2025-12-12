namespace ETicaret.Entity.Concrete
{
    public class Cart : BaseEntity
    {
        // Foreign Key
        public string UserId { get; set; } = null!;
        
        // Navigation Properties
        public virtual AppUser User { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
