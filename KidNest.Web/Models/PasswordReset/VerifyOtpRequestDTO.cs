using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.Models.PasswordReset
{
    public class VerifyOtpRequestDTO
    {
        [Required] 
        public string? EmailOrPhone { get; set; }

        [Required]
        [StringLength(6)] public string? Otp { get; set; }
    }
}
