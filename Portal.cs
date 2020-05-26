using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using CsvHelper;

namespace SuncoastBank
{
    public class Portal
    {

        private Dictionary<string, Account> Accounts;
        private FrontEnd UI;

        public Portal()
        {
            Accounts = new Dictionary<string, Account>();
            UI = new FrontEnd();
        }

        public void Login()
        {
            LoadUserDatabase();

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Welcome to the First Suncoast Bank.");
            Console.WriteLine("-----------------------------------\n");
            Console.WriteLine("Please enter your account name.\n");

            string inputAccountName = UI.PromptForString("account name");

            Console.WriteLine("-----------------------------------\n");

            Account userAccount;

            if (TryFindAccount(inputAccountName, out userAccount))
            {
                Console.WriteLine("Existing account found.\n");

                string inputExistingPassword = UI.PromptForString("existing password");

                if (!userAccount.ComparePassword(inputExistingPassword))
                {
                    Console.WriteLine("Incorrect password, good bye.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("No user with that name found.");
                Console.WriteLine("Please enter a password to create a new account.\n");

                string inputNewPassword = UI.PromptForString("new password");

                userAccount = CreateAndSaveAccount(inputAccountName, inputNewPassword);

            }

            Console.WriteLine("-----------------------------------\n");
            Console.WriteLine("Connecting you to your Suncoast Bank account.\n");

            var bankConnection = new Bank(userAccount.AccountID);
            bankConnection.Connect();
        }

        private bool TryFindAccount(string name, out Account account)
        {
            account = null;

            if (Accounts.ContainsKey(name))
            {
                account = Accounts[name];
                return true;
            }

            return false;
        }

        private Account CreateAndSaveAccount(string name, string password)
        {
            var newAccount = new Account(Accounts.Count, name, password);

            using (var writer = new StreamWriter("users.csv"))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                Accounts[newAccount.AccountName] = newAccount;
                csvWriter.WriteRecords(Accounts.Values);
            }
            return newAccount;
        }

        private void LoadUserDatabase()
        {
            if (File.Exists("users.csv"))
            {
                using (var reader = new StreamReader("users.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    foreach (var account in csv.GetRecords<Account>().ToList())
                    {
                        Accounts[account.AccountName] = account;
                    }
                }
            }
        }
    }
}
