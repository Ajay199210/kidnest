using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Users
{
    public class AdminViewModel
    {
        public int Id { get; set; }

        public string? FullName { get; set; }

        public string? EmailOrPhone { get; set; }

        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? Password { get; set; }

        public string? Code { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime? DOB { get; set; }

        [DisplayName("Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [DisplayName("Last Login PC Name")]
        public string? LastLogInPCName { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Account Updates Count")]
        public int RowVersion { get; set; }

        [DisplayName("User Updated By")]
        public string? UserUpdatedBy { get; set; }

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; }

        [DisplayName("User Created By")]
        public string? UserCreatedBy { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}
