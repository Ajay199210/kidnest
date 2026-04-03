using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.MD.Sizes
{
    public class MdSizeEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Description is required (e.g. Small, Medium..)")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Size Code is required (e.g. S, M, L..)")]
        [DisplayName("Size Code")]
        [StringLength(10, ErrorMessage = "Max 10 characters are allowed")]
        //[RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Invalid hex color code")]
        public string? SizeCode { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}
