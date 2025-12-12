using ETicaret.Entity.Concrete;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Data.Concrete
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product - Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - User relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem - Order relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // OrderItem - Product relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart - User relationship (one-to-one)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem - Cart relationship
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem - Product relationship
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint for Order Number
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Elektronik", Description = "Elektronik ürünler", CreatedDate = DateTime.Now, IsActive = true },
                new Category { Id = 2, Name = "Giyim", Description = "Giyim ürünleri", CreatedDate = DateTime.Now, IsActive = true },
                new Category { Id = 3, Name = "Ev & Yaşam", Description = "Ev ve yaşam ürünleri", CreatedDate = DateTime.Now, IsActive = true },
                new Category { Id = 4, Name = "Spor", Description = "Spor ürünleri", CreatedDate = DateTime.Now, IsActive = true },
                new Category { Id = 5, Name = "Kitap", Description = "Kitaplar", CreatedDate = DateTime.Now, IsActive = true }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "iPhone 15 Pro", Description = "Apple iPhone 15 Pro 256GB", Price = 64999.99m, Stock = 50, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/products/iphone15pro.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 2, Name = "Samsung Galaxy S24", Description = "Samsung Galaxy S24 Ultra 512GB", Price = 54999.99m, Stock = 30, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/products/galaxys24.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 3, Name = "MacBook Pro M3", Description = "Apple MacBook Pro 14 inch M3 Pro", Price = 89999.99m, Stock = 20, CategoryId = 1, IsFeatured = true, ImageUrl = "/images/products/macbookpro.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 4, Name = "Erkek Deri Ceket", Description = "Premium kalite deri ceket", Price = 2499.99m, Stock = 100, CategoryId = 2, IsFeatured = false, ImageUrl = "/images/products/dericeket.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 5, Name = "Kadın Trençkot", Description = "Şık ve zarif trençkot", Price = 1899.99m, Stock = 80, CategoryId = 2, IsFeatured = true, ImageUrl = "/images/products/trenckot.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 6, Name = "Modern Koltuk Takımı", Description = "3+2+1 modern koltuk takımı", Price = 24999.99m, Stock = 15, CategoryId = 3, IsFeatured = true, ImageUrl = "/images/products/koltuk.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 7, Name = "Koşu Ayakkabısı", Description = "Profesyonel koşu ayakkabısı", Price = 1299.99m, Stock = 200, CategoryId = 4, IsFeatured = false, ImageUrl = "/images/products/kosuayakkabisi.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 8, Name = "Yoga Matı", Description = "Premium kalite yoga matı", Price = 349.99m, Stock = 150, CategoryId = 4, IsFeatured = false, ImageUrl = "/images/products/yogamati.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 9, Name = "Suç ve Ceza", Description = "Dostoyevski - Suç ve Ceza", Price = 89.99m, Stock = 500, CategoryId = 5, IsFeatured = false, ImageUrl = "/images/products/sucveceza.jpg", CreatedDate = DateTime.Now, IsActive = true },
                new Product { Id = 10, Name = "1984", Description = "George Orwell - 1984", Price = 79.99m, Stock = 400, CategoryId = 5, IsFeatured = true, ImageUrl = "/images/products/1984.jpg", CreatedDate = DateTime.Now, IsActive = true }
            );
        }
    }
}
