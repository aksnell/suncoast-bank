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
        private FrontEnd GUI;

        public Portal()
        {
            Accounts = new Dictionary<string, Account>();
            GUI = new FrontEnd();
        }

        public void Login()
        {
            LoadUserDatabase();

            Console.WriteLine("-----------------------------------");
            Console.WriteLine("Welcome to the First Suncoast Bank!");
            Console.WriteLine("Your home for simple, abstraction free banking!");
            Console.WriteLine("-----------------------------------\n");
            Console.WriteLine("Please enter your account name!\n");
            string inputAccountName = GUI.PromptForString("account name");

            Account userAccount;

            Console.WriteLine("-----------------------------------\n");
            if (TryFindAccount(inputAccountName, out userAccount))
            {
                Console.WriteLine("Account found!\n");
                string inputExistingPassword = GUI.PromptForString("enter your password");

                if (!userAccount.ConfirmPassword(inputExistingPassword))
                {
                    Console.WriteLine("Incorrect password, good bye!");
                    return;
                }
            }
            else
            {
                Console.WriteLine("No user with that name found!");
                Console.WriteLine("Please enter a password to create a new account!\n");
                string inputNewPassword = GUI.PromptForString("enter new password");

                userAccount = CreateAndSaveAccount(inputAccountName, inputNewPassword);

            }
            Console.WriteLine("-----------------------------------\n");
            Console.WriteLine("Success! Connecting you to your Suncoast Bank account!\n");

            var bankConnection = new BankConnection(userAccount.AccountID);
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
            Accounts[newAccount.AccountName] = newAccount;

            var writer = new StreamWriter("users.csv");
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(Accounts.Values);

            writer.Close();

            return newAccount;
        }

        private void LoadUserDatabase()
        {
            if (File.Exists("users.csv"))
            {
                var reader = new StreamReader("users.csv");
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

                foreach (var account in csvReader.GetRecords<Account>().ToList())
                {
                    Accounts[account.AccountName] = account;
                }

            }
        }
    }
}
