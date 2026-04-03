namespace KidNest.Web.ViewModels.Products
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public int? ColorId { get;set ; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public int? SizeId { get; set; }
        public string? SizeCode { get; set; }
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
    }
}
