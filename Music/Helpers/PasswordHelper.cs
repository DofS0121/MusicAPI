using System.Security.Cryptography;
using System.Text;

namespace Music.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
