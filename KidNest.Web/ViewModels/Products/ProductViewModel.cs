using KidNest.Web.ViewModels.MD.Colors;
using KidNest.Web.ViewModels.MD.Sizes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Products
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [DisplayName("Category")]
        public string? CategoryName { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal? Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:0}%")]
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }

        [DisplayName("Product Image")]
        public string? ImagePath { get; set; }

        [DisplayName("New Release Until")]
        public DateTime? NewReleaseUntil { get; set; }

        //[DisplayName("Available Colors")]
        //public List<MdColorViewModel> Colors { get; set; } = [];

        //[DisplayName("Available Sizes")]
        //public List<MdSizeViewModel> Sizes { get; set; } = [];

        [DisplayName("Product Variants")]
        public List<ProductVariantViewModel> Variants { get; set; } = [];

        // Calculated property: distinct colors from ProductVariants
        public List<(string? Color, string? ColorHex, int ColorId)> DistinctColors =>
            Variants
                .Where(v => !string.IsNullOrEmpty(v.Color) && 
                    !string.IsNullOrEmpty(v.ColorHex) && v.ColorId.HasValue)
                .GroupBy(v => new { v.Color, v.ColorHex, v.ColorId })
                .Select(g => (g.Key.Color, g.Key.ColorHex, g.Key.ColorId!.Value))
                .ToList();

        // Calculated property: distinct sizes from ProductVariants
        public List<(string? SizeCode, int SizeId)> DistinctSizes =>
            Variants
                .Where(v => v.SizeCode != null && v.SizeId.HasValue)
                .GroupBy(v => new { v.SizeCode, v.SizeId })
                .Select(g => (g.Key.SizeCode, g.Key.SizeId!.Value))
                .ToList();

        [DisplayName("Discounted Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal? DiscountedPrice => Discount > 0 ? Price * (1 - Discount / 100) : Price;
    }
}
