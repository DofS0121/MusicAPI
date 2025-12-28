using System.ComponentModel.DataAnnotations;

namespace Music.Models
{
    public class PasswordOtp
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OTP { get; set; } = string.Empty;
        public DateTime ExpireAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
