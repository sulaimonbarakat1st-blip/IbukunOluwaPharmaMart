using System.ComponentModel.DataAnnotations.Schema;

namespace IbukunOluwaPharmaMart.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } = 0;

        public int Quantity { get; set; }
        public int LowStockLevel { get; set; } = 10;

        public string? NAFDACNo { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Dosage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal Price { get { return SellingPrice; } set { SellingPrice = value; } }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public string? ImageUrl { get; set; }
    }
}