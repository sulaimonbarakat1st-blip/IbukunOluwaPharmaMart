namespace IbukunOluwaPharmaMart.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Cashier";
        public string FullName { get; set; } = "";
    }
}