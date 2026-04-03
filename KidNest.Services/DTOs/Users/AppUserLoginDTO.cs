namespace KidNest.Services.DTOs.Users
{
    public class AppUserLoginDTO
    {
        public string EmailOrPhone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
