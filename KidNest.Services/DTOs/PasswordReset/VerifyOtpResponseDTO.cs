namespace KidNest.Web.Models.PasswordReset
{
    public class VerifyOtpResponseDTO
    {
        public bool IsSuccess { get; set; }
        public bool IsLockedOut { get; set; }
        public int RemainingAttempts { get; set; }
        public double? LockoutTimeRemaining { get; set; } // In minutes
        public string? Message { get; set; } // For UI or logging
    }
}
