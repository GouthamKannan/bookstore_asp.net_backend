using System;
using System.Linq;

using BookStore.Models;

namespace BookStore.Helper
{
    public class JwtHandler
    {
        static readonly string AllChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        // Generate random string for JWT Token
        private static string GenerateRandomString(int Length)
        {
            var random = new Random();
            string Token = new string(
                Enumerable.Repeat(AllChars, Length)
                .Select(token => token[random.Next(token.Length)]).ToArray());

            return Token.ToString();
        }

        // Create random JWT token
        public static string CreateToken(string email)
        {
            string token = null;
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault();
            while (true)
            {
                token = GenerateRandomString(64);
                user = dbContext.users.FirstOrDefault(cur_user => cur_user.jwt_token == token);
                if (user == null)
                {
                    break;
                }
            }

            // Store the token in database
            user = dbContext.users.FirstOrDefault(cur_user => cur_user.email == email);
            user.jwt_token = token;
            dbContext.SaveChanges();

            return token;
        }

        // Verify whether a JWT token is valid
        public static bool VerifyToken(string user_name, string token)
        {
            System.Diagnostics.Debug.WriteLine(user_name);
            System.Diagnostics.Debug.WriteLine(token);
            if (user_name == null || token == null)
            {
                return true;
            }

            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.user_name == user_name);
            if (user != null)
            {
                if (user.jwt_token == token)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        // Clear the JWT token from the database (during logout)
        public static void ClearToken(string user_name)
        {
            BookStoreEntities dbContext = new BookStoreEntities();
            var user = dbContext.users.FirstOrDefault(cur_user => cur_user.user_name == user_name);
            if (user != null)
            {
                user.jwt_token = null;
                dbContext.SaveChanges();
            }
        }
    }
}