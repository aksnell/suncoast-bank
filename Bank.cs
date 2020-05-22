using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using CsvHelper;

namespace SuncoastBank
{
    public class Bank
    {
        private List<Transaction> Transactions = new List<Transaction>();
        private TextInfo Formatter = new CultureInfo("en-Us", false).TextInfo;

        // Main interaction loop
        public void Connect()
        {
            // Pull old transactions from file.
            LoadTransactions();
            bool isConnected = true;

            // You can have as many users as you want!
            string name = PromptForString("what is your name?");

            // Getting initial user information.
            var userTransactions = Transactions.Where(transaction => transaction.Name == name).ToList();
            int userCheckings = CalculateAccountBalance("Checkings", name);
            int userSavings = CalculateAccountBalance("Savings", name);

            // Main menu choices
            List<string> validChoices = new List<string>
            {
                "View Balance",
                "Withdraw",
                "Deposit",
                "Transfer",
                "Quit"
            };

            // Account choices
            List<String> validAccounts = new List<string>
            {
                "Checkings",
                "Savings"
            };

            // Welcome message.
            Console.WriteLine("Welcome to the First Suncoast Bank!");
            Console.WriteLine("Your home for simple, two class banking!");
            Console.WriteLine("'Abstraction free since 2020!'");
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"You are connected as: {name}");
            Console.WriteLine($"I sure hope thats who you are!");
            Console.WriteLine("-------------------------------");

            // Display initial balance if available, otherwise insult you.
            if (userTransactions.Count == 0)
            {
                Console.WriteLine("You have no transaction history with us!");
                Console.WriteLine("Deposit some cash or stop waisting our time!");
            } else {
                WriteList("balances", new List<string> {$"Checkings: {userCheckings}", $"Savings: {userSavings}"});
            }
            Console.WriteLine("-------------------------------");

            // Begin interaction
            while (isConnected)
            {
                int userInput = PromptFromList("choose one of the following", validChoices);

                switch (userInput)
                {
                    // View balances
                    case 0:
                    {
                        userCheckings = CalculateAccountBalance("Checkings", name);
                        userSavings = CalculateAccountBalance("Savings", name);

                        WriteList("balances", new List<string> {$"Checkings: {userCheckings}", $"Savings: {userSavings}"});
                        break;
                    }

                    // Withdraw balance
                    case 1:
                    {
                        int accountChoice = PromptFromList("which account", validAccounts);
                        int amountToWithdraw = PromptForInteger("how much to withdraw");

                        WithdrawFromAccount(validAccounts[accountChoice], amountToWithdraw, name);
                        break;
                    }

                    // Deposit balance
                    case 2:
                    {
                        int accountChoice = PromptFromList("which account", validAccounts);
                        int amountToDeposit = PromptForInteger("how much to deposit");

                        DepositToAccount(validAccounts[accountChoice], amountToDeposit, name);
                        break;
                    }
                    // Transfer balance
                    case 3:
                    {
                        int accountChoice = PromptFromList("to which account", validAccounts);
                        int amountToDeposit = PromptForInteger("how much to transfer");

                        TransferToAccount(validAccounts[accountChoice], amountToDeposit, name);
                        break;
                    }
                    // Quit
                    case 4:
                    {
                        Console.WriteLine("Goodbye!");
                        isConnected = false;
                        break;
                    }
                }
            }
        }

        // Query current transactions to determine current balance for passed account and user.
        // What is the preferred way to format long linq queries?
        private int CalculateAccountBalance(string accountType, string name)
        {
            return Transactions.Where(transaction => transaction.Name == name && transaction.AccountType == accountType).Select(transaction => transaction.AccountDelta).Sum();
        }

        // Subtracts amount from accountType if amount > 0 and <= CalculateAccountBalance
        // appends relavent transaction to list and saves new list to file.
        private void WithdrawFromAccount(string accountType, int amount, string name)
        {
            if (amount < 0)
            {
                Console.WriteLine("You can't withdraw a negative amount!");
                return;
            }

            if (amount > CalculateAccountBalance(accountType, name))
            {
                Console.WriteLine($"You don't have that much to withdraw from that account!");
                return;
            }
            Console.WriteLine($"You withdrew ${amount} from your {accountType} account!");

            var newTransaction = new Transaction{};

            newTransaction.AccountDelta = -amount;
            newTransaction.AccountType = accountType;
            newTransaction.Name = name;

            Transactions.Add(newTransaction);

            SaveTransactions();
        }

        // Adds amount to accountType if amount > 0
        // appends relavent transaction to list and saves new list to file.
        private void DepositToAccount(string accountType, int amount, string name)
        {
            if (amount < 0)
            {
                Console.WriteLine("You can't deposit a negative amount!");
            }

            Console.WriteLine($"You desposited ${amount} to your {accountType} account!");

            var newTransaction = new Transaction{};

            newTransaction.AccountDelta = amount;
            newTransaction.AccountType = accountType;
            newTransaction.Name = name;

            Transactions.Add(newTransaction);

            SaveTransactions();
        }

        private void TransferToAccount(string accountType, int amount, string name)
        {
            if (amount < 0)
            {
                Console.WriteLine("You can't transfer a negative amount!");
            }

            string transferAccountType = (accountType == "Checkings") ? "Savings" : "Checking";

            if (CalculateAccountBalance(transferAccountType, name) < amount)
            {
                Console.WriteLine($"You don't have that much to transfer from that account!");
            }

            DepositToAccount(accountType, amount,  name);
            WithdrawFromAccount(transferAccountType, amount, name);
        }

        // Attempts to load previous transactions from local CSV file.
        public void LoadTransactions()
        {
            if (File.Exists("transactions.csv"))
            {
                var reader = new StreamReader("transactions.csv");
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

                Transactions = csvReader.GetRecords<Transaction>().ToList();
            }
        }

        // Saves current transactions to CSV file.
        // Called after DepositToAccount and WithdrawFromAccount.
        public void SaveTransactions()
        {
            var writer = new StreamWriter("transactions.csv");
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvWriter.WriteRecords(Transactions);

            writer.Close();
        }

        // Front end function
        public int PromptFromList(string label, List<string> choices)
        {
            if (choices.Count() == 0)
            {
                Console.WriteLine("No matching choices!");
                return -1;
            }

            WriteList(label, choices);

            var userInput = PromptForInteger("choose", choices.Count);

            while (userInput == -1)
            {
                Console.WriteLine("Please make a valid selection!");
                userInput = PromptForInteger("choose", choices.Count);
            }

            return userInput - 1;

        }

        // Front end function
        public int PromptForInteger(string label, int max = Int32.MaxValue)
        {
            WriteLabel(label);

            int userInput;
            var validInput = Int32.TryParse(Console.ReadLine(), out userInput);

            if (validInput && userInput <= max && userInput > 0)
            {
                return userInput;
            }

            return -1;
        }

        // Front end function
        public string PromptForString(string label)
        {
            WriteLabel(label);
            return Console.ReadLine();
        }

        // Front end function
        public void WriteList(string label, List<string> choices)
        {
            int ordinal = 1;
            string formattedList = $"{Formatter.ToTitleCase(label)}";

            foreach (string choice in choices)
            {
                formattedList += $"\n\t({ordinal}) {choice}";
                ordinal++;
            }

            Console.WriteLine(formattedList);
        }

        // Front end function
        public void WriteLabel(string label)
        {
            Console.Write($"{Formatter.ToTitleCase(label)}: ");
        }

    }

}