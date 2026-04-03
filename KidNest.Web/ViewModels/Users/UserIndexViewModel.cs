using System.ComponentModel;

namespace KidNest.Web.ViewModels.Users
{
    public class UserIndexViewModel
    {
        public int Id { get; set; }
        
        [DisplayName("Full Name")]
        public string? FullName { get; set; }
        
        [DisplayName("Phone Number")]
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime? DOB { get; set; }
        public string? Code { get; set; }
        public string? Password { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLogInPCName { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }
        public int RowVersion { get; set; }
        public string? UserUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? UserCreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
