using System.Security.Cryptography;
using System.Text;

namespace SkillBridge.Common;

public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var hashOfInput = HashPassword(password);
        return string.Equals(hashOfInput, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
