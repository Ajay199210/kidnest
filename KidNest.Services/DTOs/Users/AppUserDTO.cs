namespace KidNest.Services.DTOs.Users
{
    public class AppUserDTO
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DOB { get; set; }
        public string? Code { get; set; }
        public string? Password { get; set; }
        public string? NewPassword { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? LastLogInPCName { get; set; }
        public bool? IsActive { get; set; }
        public int RowVersion { get; set; }
        public string? UserUpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? UserCreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Role { get; set; }
    }
}
