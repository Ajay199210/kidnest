using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Categories
{
    public class CategoryEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "Max 200 characters are allowed")]
        public string? Description { get; set; }
    }
}
