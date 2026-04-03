using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Users
{
    public class UserDetailsViewModel
    {
        public int Id { get; set; }

        [DisplayName("Full Name")]
        public string? FullName { get; set; }

        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime? DOB { get; set; }
        public string? Code { get; set; }
        public string? Password { get; set; }

        [DisplayName("Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [DisplayName("Last Login PC Name")]
        public string? LastLogInPCName { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Row Version")]
        public int RowVersion { get; set; }

        [DisplayName("Account Updated By")]
        public string? UserUpdatedBy { get; set; }

        [DisplayName("Last Updated By")]
        public DateTime? LastUpdated { get; set; }

        [DisplayName("Account Created By")]
        public string? UserCreatedBy { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
        public string? Role { get; set; }
    }
}
