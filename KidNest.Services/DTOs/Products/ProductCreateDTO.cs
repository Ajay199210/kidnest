namespace KidNest.Services.DTOs.Products
{
    public class ProductCreateDTO
    {
        public int CategoryId { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath { get; set; }
        public bool IsNewRelease { get; set; }
        public DateTime NewReleaseUntil { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<int> SelectedColorIds { get; set; } = [];
        public List<int> SelectedSizeIds { get; set; } = [];
        public List<ProductVariantCreateDTO> VariantCreateDTOs { get; set; } = [];
    }
}
