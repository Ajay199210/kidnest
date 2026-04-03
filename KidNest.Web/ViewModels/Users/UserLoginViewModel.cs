using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Users
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "Email or phone number is required")]
        [DisplayName("Email or Phone")]
        public string? EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }

    }
}
