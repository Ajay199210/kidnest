using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Users
{
    public class UserCreateViewModel
    {
        [DisplayName("Full Name")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? FullName { get; set; }

        [DisplayName("Phone Number")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? PhoneNumber { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Email { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime? DOB { get; set; }

        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? Code { get; set; }
        public string? Password { get; set; }

        [DisplayName("Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [DisplayName("Last Login PC Name")]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? LastLogInPCName { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        [DisplayName("Row Version")]
        public int RowVersion { get; set; }

        [DisplayName("User Updated By")]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? UserUpdatedBy { get; set; }

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [DisplayName("User Created By")]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? UserCreatedBy { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}
