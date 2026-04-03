namespace KidNest.Web.ViewModels.Components
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public int? VariantId { get; set; }
        public int? ColorId { get; set; }
        public string? Color {  get; set; }
        public int? SizeId { get; set; }
        public string? Size { get; set; }
        public int Quantity { get; set; } // Ordered quantity
        public int StockQuantity { get; set; }
    }
}
