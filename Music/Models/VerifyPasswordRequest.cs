namespace Music.Models
{
    public class VerifyPasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = "";
    }
}
