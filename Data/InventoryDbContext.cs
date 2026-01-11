using Microsoft.EntityFrameworkCore;
using InventoryManagementSystem.Models;

namespace InventoryManagementSystem.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) 
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table names mapping
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Inventory>().ToTable("Inventory");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Supplier>().ToTable("Suppliers");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<ActivityLog>().ToTable("ActivityLogs");

            // Composite or computed columns
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasComputedColumnSql("[Quantity] * [UnitPrice]");

            base.OnModelCreating(modelBuilder);
        }
    }
}