using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Users
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Email or phone number is required")]
        //[StringLength(50)]
        [DisplayName("Email or phone")]
        public string? EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        //[StringLength(100)]
        public string? Password { get; set; }

        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }
    }
}
