using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using KidNest.Web.Models;

namespace KidNest.Web.ViewModels.Products
{
    public class ProductEditViewModel : IProductVariantDropdowns
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Category")]
        public int CategoryId { get; set; }  // Selected from dropdown

        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? Barcode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "Max 200 characters are allowed")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(0, 100)]
        public decimal? Discount { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [DisplayName("Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ImagePath { get; set; }
        
        [DisplayName("Select colors")]
        public List<int> SelectedColorIds { get; set; } = [];

        [DisplayName("Select sizes")]
        public List<int> SelectedSizeIds { get; set; } = [];

        [DisplayName("New Release")]
        public bool IsNewRelease { get; set; }

        [DisplayName("New Release End Date")]
        public DateTime? NewReleaseUntil { get; set; }

        // Select List Data
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IEnumerable<SelectListItem> Colors { get; set; } = [];
        public IEnumerable<SelectListItem> Sizes { get; set; } = [];

        public List<ProductVariantInputModel> Variants { get; set; } = [];
    }
}
