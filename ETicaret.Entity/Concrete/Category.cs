namespace ETicaret.Entity.Concrete
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        
        // Navigation Property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
