using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Services.DTOs.Users
{
    public class AdminLoginDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 5)]
        [DisplayName("Email or phone")]
        public string EmailOrPhone { get; set; }  // Either Email or Phone number

        [Required]
        //[StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
