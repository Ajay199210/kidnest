using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Users
{
    public class AdminEditViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        [DisplayName("Full Name")]
        public string? FullName { get; set; }

        [DisplayName("Phone Number")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? PhoneNumber { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Main password is required to apply changes")]
        public string? Password { get; set; }
    }
}
