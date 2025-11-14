using System;
using BCrypt.Net;

namespace TodoListMVC.Helpers
{
    /// <summary>
    /// Helper class ?? hash và verify password s? d?ng BCrypt
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash password v?i BCrypt (work factor 11)
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>BCrypt hashed password</returns>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
        }

        /// <summary>
        /// Verify password v?i hash ?ã l?u
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">BCrypt hashed password</param>
        /// <returns>True n?u password kh?p</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// T?o sample hashed passwords cho testing
        /// Console app ?? ch?y:
        /// 
        /// using TodoListMVC.Helpers;
        /// 
        /// Console.WriteLine("password123: " + PasswordHelper.HashPassword("password123"));
        /// Console.WriteLine("admin123: " + PasswordHelper.HashPassword("admin123"));
        /// </summary>
        public static void GenerateSampleHashes()
        {
            Console.WriteLine("=== SAMPLE PASSWORD HASHES ===");
            Console.WriteLine($"password123: {HashPassword("password123")}");
            Console.WriteLine($"admin123: {HashPassword("admin123")}");
            Console.WriteLine($"test123: {HashPassword("test123")}");
        }
    }
}
