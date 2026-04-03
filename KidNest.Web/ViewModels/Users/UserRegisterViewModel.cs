using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Users
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Max of 50 characters are allowed")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email or phone number is required")]
        [DisplayName("Email or Phone")]
        [RegularExpression(@"(^[^@\s]+@[^@\s]+\.[^@\s]+$)|(^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$)",
            ErrorMessage = "Must be email or phone number")]
        public string? EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Delivery Address is required")]
        [StringLength(50, ErrorMessage = "Max of 50 characters are allowed")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DisplayName("Confirm Password")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
