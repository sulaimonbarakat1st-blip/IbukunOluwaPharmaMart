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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Pharmacy" },
                new Category { Id = 2, Name = "Supermarket" },
                new Category { Id = 3, Name = "Drinks" },
                new Category { Id = 4, Name = "Toiletries" }
            );
        }
    }
}