using System.ComponentModel.DataAnnotations.Schema;

namespace IbukunOluwaPharmaMart.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public List<SaleItem>? Items { get; set; }
    }

    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        // Compatibility for old code
        [NotMapped] public int Qty { get { return Quantity; } set { Quantity = value; } }
        [NotMapped] public decimal Price { get { return SellingPrice; } set { SellingPrice = value; } }
        [NotMapped] public decimal UnitPrice { get { return SellingPrice; } set { SellingPrice = value; } }
    }
}