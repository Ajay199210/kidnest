using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.Models.PasswordReset
{
    public class VerifyUserRequestDTO
    {
        [Required(ErrorMessage = "Please enter a valid phone number")]
        public string? EmailOrPhone { get; set; }
    }
}
