using KidNest.Web.ViewModels.MD.Colors;
using KidNest.Web.ViewModels.MD.Sizes;
using KidNest.Web.ViewModels.Products;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Home
{
    public class ProductCardViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string? ImagePath { get; set; }
        
        public int Quantity { get; set; }

        public List<ProductVariantViewModel> Variants { get; set; } = [];

        // Calculated property: distinct colors from ProductVariants
        public List<(string? Color, string? ColorHex, int ColorId)> DistinctColors =>
            Variants
                .Where(v => !string.IsNullOrEmpty(v.Color) && !string.IsNullOrEmpty(v.ColorHex) && v.ColorId.HasValue)
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

        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                    return "N/A";

                var span = Description.AsSpan();

                return span.Length > 105 ? $"{span[..100]}..." : Description;
            }
        }

        [DisplayFormat(DataFormatString = "{0:F2}")]
        public decimal FinalPrice => Discount.HasValue 
            ? Math.Round(Price * (1 - Discount.Value / 100), 2) 
            : Price;
    }
}
