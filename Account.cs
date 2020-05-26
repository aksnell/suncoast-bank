using System;
using System.Linq;

namespace SuncoastBank
{
    public class Account
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }

        public Account() {}
        public Account(int accountID, string accountName, string password)
        {
            var rng = new Random();

            AccountID = accountID;
            AccountName = accountName;
            PasswordSalt = string.Join("", "0123456789".ToCharArray().Select(salt => salt = (char)(rng.Next(65, 90))));
            PasswordHash = HashPassword(password);
        }

        public bool ComparePassword(string password)
        {
            return PasswordHash == HashPassword(password);
        }

        // 16 bit hash, i.e a single UTF-16 character
        private string HashPassword(string password)
        {
            char hashedPassword = 'A';
            string saltedPassword = PasswordSalt + password;

            foreach (char passChar in saltedPassword)
            {
                hashedPassword ^= passChar;
            }

            return hashedPassword.ToString();
        }

    }
}
