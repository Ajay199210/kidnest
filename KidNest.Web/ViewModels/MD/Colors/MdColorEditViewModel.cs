using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.MD.Colors
{
    public class MdColorEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Color name is required")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Color value is required")]
        [DisplayName("Color")]
        [StringLength(7, ErrorMessage = "Use a valid hex color code, e.g. #FF5733")]
        //[RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Invalid hex color code")]
        public string? HexValue { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}
