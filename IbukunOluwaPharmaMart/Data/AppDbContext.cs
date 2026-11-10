using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Models;

namespace IbukunOluwaPharmaMart.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<SuspendedSale> SuspendedSales { get; set; }
        public DbSet<SuspendedSaleItem> SuspendedSaleItems { get; set; }
        public DbSet<Staff> Staffs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Pharmacy" },
                new Category { Id = 2, Name = "Supermarket" },
                new Category { Id = 3, Name = "Drinks" },
                new Category { Id = 4, Name = "Toiletries" }
            );

            modelBuilder.Entity<SuspendedSale>().Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SuspendedSaleItem>().Property(i => i.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Sale>().Property(s => s.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Sale>().Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Sale>().Property(s => s.Balance).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<SaleItem>().Property(i => i.Price).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>().Property(p => p.RetailPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.WholesalePrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.SellingPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
        }
    }
}