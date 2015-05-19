using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Sadik.Models
{
    public class Password
    {
        private static int RandomPasswordLength = 12;
        public bool Matches(string clearText)
        {
            var probedPasswordHash = new Rfc2898DeriveBytes(clearText, Salt).GetBytes(HashSize);
            return Hash.SequenceEqual(probedPasswordHash);
        }

        public Password(string clearText)
        {
            var deriveBytes = new Rfc2898DeriveBytes(clearText, SaltSize);
            Hash = deriveBytes.GetBytes(HashSize);
            Salt = deriveBytes.Salt;
        }

        public Password(byte[] hash, byte[] salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public Password()
            : this("")
        {
        }

        public static string GenerateRandom()
        {
            var password = "";
            var characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rand = new Random();
            for (int i = 0; i < RandomPasswordLength; i++)
            {
                password += characters[rand.Next(0, characters.Length)];
            }
            return password;
        }

        public byte[] Hash { get; private set; }

        public byte[] Salt { get; private set; }

        const int SaltSize = 16;
        const int HashSize = 128;
    }
}