using System.Security.Cryptography;
using System.Text;

namespace CLDV6212P1.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Hashes a password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
    }
}

