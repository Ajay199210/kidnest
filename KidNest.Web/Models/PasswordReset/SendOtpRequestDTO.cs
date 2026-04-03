using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.Models.PasswordReset
{
    public class SendOtpRequestDTO
    {
        [Required]
        public string? EmailOrPhone { get; set; }
    }
}
