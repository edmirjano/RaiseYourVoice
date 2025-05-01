using Microsoft.Extensions.Configuration;
using RaiseYourVoice.Application.Interfaces;
using System;
using System.Security.Cryptography;

namespace RaiseYourVoice.Infrastructure.Services.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly int _iterations;
        
        public PasswordHasher(IConfiguration configuration)
        {
            _iterations = Convert.ToInt32(configuration["SecuritySettings:PasswordHashingIterations"] ?? "10000");
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Retrieve the stored hash
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Extract the salt from the stored hash
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute the hash for the provided password
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, _iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // Compare the computed hash with the stored hash
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}