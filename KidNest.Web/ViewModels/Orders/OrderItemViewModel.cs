namespace KidNest.Web.ViewModels.Orders
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public string? Size { get; set; }

        // Calculated
        public decimal DiscountedPrice => Price * (1 - Discount / 100);
        public decimal TotalPrice => Quantity * DiscountedPrice;
    }
}
