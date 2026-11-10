using System.ComponentModel.DataAnnotations;

namespace IbukunOluwaPharmaMart.Models
{
    public class Sale
    {
        public int Id { get; set; }

        [Display(Name = "Receipt No")]
        public string ReceiptNo { get; set; } = "";

        public DateTime Date { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        // --- NEW FOR CREDIT SALES ---
        public string PaymentType { get; set; } = "Cash"; // Cash, Transfer, Credit
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; } // Amount owed
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public bool IsFullyPaid { get; set; } = true;

        public List<SaleItem> Items { get; set; } = new();
    }

    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }

        public Product? Product { get; set; }
        public Sale? Sale { get; set; }
    }
}