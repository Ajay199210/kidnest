using KidNest.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Products
{
    public class ProductCreateViewModel : IProductVariantDropdowns
    {
        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }  // Selected from dropdown

        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? Barcode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "Max 500 characters are allowed")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, 100)]
        public decimal? Discount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be zero or a positive number")]
        public int Quantity { get; set; }

        [DisplayName("Image")]
        public IFormFile? ImageFile { get; set; }

        [DisplayName("Select colors")]
        public List<int> SelectedColorIds { get; set; } = [];

        [DisplayName("Select sizes")]
        public List<int> SelectedSizeIds { get; set; } = [];

        [DisplayName("New Release")]
        public bool IsNewRelease { get; set; }

        // Select List Data
        public IEnumerable<SelectListItem> Categories { get; set; } = [];
        public IEnumerable<SelectListItem> Colors { get; set; } = [];
        public IEnumerable<SelectListItem> Sizes { get; set; } = [];

        public List<ProductVariantInputModel> Variants { get; set; } = [];
    }
}
