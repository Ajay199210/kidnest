namespace KidNest.Web.Models.PasswordReset
{
    public class GenerateOtpResponseDTO
    {
        public bool IsSuccess { get; set; }
        public bool IsLockedOut { get; set; }
        public double? LockoutTimeRemaining { get; set; } // In minutes
        public string? Message { get; set; } // For UI or logging
        public string? OtpCode { get; set; }
    }
}
