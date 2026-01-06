using Microsoft.EntityFrameworkCore;
using PrivateECommerce.API.Models;
using System.Collections.Generic;

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
    }
}
