namespace KidNest.Web.ViewModels.Products
{
    public class ProductIndexViewModel
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
    }
}
