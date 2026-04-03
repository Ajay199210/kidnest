using KidNest.Core.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Contents
{
    public class ContentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Content name is required")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Content type is required")]
        [Display(Name = "Content Type")]
        public ContentType? Type { get; set; }

        public string? Path { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        public IFormFile? File { get; set; }
    }
}
