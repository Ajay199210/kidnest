using KidNest.Services.DTOs.MD;

namespace KidNest.Services.DTOs.Products
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string? Barcode { get; set; }
        public string? ImagePath { get; set; }
        public bool? IsNewRelease { get; set; }
        public DateTime? NewReleaseUntil { get; set; }

        //public List<MdColorDTO> Colors { get; set; } = [];
        //public List<MdSizeDTO> Sizes { get; set; } = [];
        public List<int> SelectedColorIds { get; set; } = [];
        public List<int> SelectedSizeIds { get; set; } = [];

        public List<ProductVariantDTO> VariantDTOs { get; set; } = [];

        public decimal? FinalPrice => Discount > 0 ? Price * (1 - Discount / 100) : (Price ?? 0m);
    }
}
