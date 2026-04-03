namespace KidNest.Services.DTOs.Orders
{
    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public string? SizeCode { get; set; }

        // Calculated
        public decimal DiscountedPrice => Price * (1 - Discount / 100);
        public decimal TotalPrice => Quantity * DiscountedPrice;
    }
}
