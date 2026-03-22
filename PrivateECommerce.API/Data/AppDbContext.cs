using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Models;

namespace PrivateECommerce.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<ProductStock> ProductStocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= CART ↔ CART ITEMS =================
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ProductImage>()
    .HasOne(pi => pi.Product)
    .WithMany(p => p.Images)
    .HasForeignKey(pi => pi.ProductId)
    .OnDelete(DeleteBehavior.Cascade);



            // ================= CART ↔ USER =================
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================= CART ITEM ↔ PRODUCT VARIANT =================
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ProductVariant)
                .WithMany()
                .HasForeignKey(ci => ci.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);

         
            // ================= USER =================
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.SalesExecutive)
                .WithMany(se => se.AssignedCustomers)
                .HasForeignKey(u => u.SalesExecutiveId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= ORDER ↔ USER (CUSTOMER) =================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.OrdersPlaced)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= ORDER ↔ USER (SALES EXECUTIVE) =================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.SalesExecutive)
                .WithMany(u => u.OrdersHandled)
                .HasForeignKey(o => o.SalesExecutiveId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================= 🔥 ORDER INDEXES =================
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedAt);
            modelBuilder.Entity<Order>()
        .Property(o => o.Status)
        .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.Status, o.CreatedAt });

            // ================= PRODUCT ↔ VARIANT =================
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId);

            // ================= PRODUCT VARIANT =================
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(v => v.ProductCode)
                .IsUnique();

        }

    }

}
