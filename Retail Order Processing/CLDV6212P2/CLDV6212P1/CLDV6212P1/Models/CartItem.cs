namespace CLDV6212P1.Models
{
    public class CartItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Size { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string ImageUrl { get; set; }

        public double TotalPrice => UnitPrice * Quantity;
    }
}
