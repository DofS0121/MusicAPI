namespace Music.Models
{
    public class VerifyOtpRequest
    {
        public int UserId { get; set; }
        public string OTP { get; set; } = "";
    }
}
