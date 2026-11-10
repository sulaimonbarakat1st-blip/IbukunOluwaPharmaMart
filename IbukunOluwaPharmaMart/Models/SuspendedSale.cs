using System.ComponentModel.DataAnnotations;

namespace IbukunOluwaPharmaMart.Models
{
    public class SuspendedSale
    {
        public int Id { get; set; }
        public string SuspendedNo { get; set; } = ""; // e.g SUSP-20231021-001
        public DateTime DateSuspended { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        public List<SuspendedSaleItem> Items { get; set; } = new();
    }

    public class SuspendedSaleItem
    {
        public int Id { get; set; }
        public int SuspendedSaleId { get; set; }
        public SuspendedSale? SuspendedSale { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal => Quantity * Price;
    }
}