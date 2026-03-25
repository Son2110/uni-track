using System.Security.Cryptography;
using System.Text;

namespace PMSS.Infrastructure.Utilities;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        var hash = ComputeHash(password, salt);

        var hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);

            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            var computedHash = ComputeHash(password, salt);

            return CryptographicEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
    }

    private static bool CryptographicEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
