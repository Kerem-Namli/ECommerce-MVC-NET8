using ETicaret.Entity.Concrete;

namespace ETicaret.Data.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Product> Products { get; }
        IRepository<Category> Categories { get; }
        IRepository<Order> Orders { get; }
        IRepository<OrderItem> OrderItems { get; }
        IRepository<Cart> Carts { get; }
        IRepository<CartItem> CartItems { get; }
        
        Task<int> SaveChangesAsync();
        int SaveChanges();
    }
}
