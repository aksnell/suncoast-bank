using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using CsvHelper;

namespace SuncoastBank
{
    public struct AccountType
    {
        public const int
            CHECKINGS = 0,
            SAVINGS = 1;
    }

    public struct AccountAction
    {
        public const int
            DEPOSIT = 0,
            WITHDRAWAL = 1;
    }

    public struct AccountError
    {
        public const int
            NONE = 0,
            UNDERFLOW = 1,
            OVERFLOW = 2;
    }

    public class BankConnection
    {
        private int ConnectedAccountID;
        private List<Transaction> ConnectedAccountTransactions = new List<Transaction>();
        private FrontEnd UI;

        public BankConnection(int accountID)
        {
            ConnectedAccountID = accountID;
            UI = new FrontEnd();
        }

        List<string> validChoices = new List<string>
        {
            "View Balance",
            "Withdraw",
            "Deposit",
            "Transfer",
            "Quit"
        };

        List<string> validAccounts = new List<string>
        {
            "Checkings",
            "Savings"
        };

        public void Connect()
        {

            if (TryLoadAccountTransactions(out ConnectedAccountTransactions))
            {
                Console.WriteLine("We are glad to see you again!\n");
                DisplayBalances();
            }
            else
            {
                Console.WriteLine("Welcome to your new account with Suncoast Bank!\n");
            }


            bool isConnected = true;

            while (isConnected)
            {
                int userInput = UI.PromptFromList("choose one of the following", validChoices);

                switch (userInput)
                {
                    // View balances
                    case 0:
                        {
                            DisplayBalances();
                            break;
                        }

                    // Withdraw balance
                    case 1:
                        {
                            int accountChoice = UI.PromptFromList("which account", validAccounts);
                            int amountToWithdraw = UI.PromptForInteger("how much to withdraw");

                            var error = WithdrawFrom(accountChoice, amountToWithdraw);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot withdraw a negative amount!\n");
                                    break;
                                case AccountError.OVERFLOW:
                                    Console.WriteLine("You don't have that much to withdraw!\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully withdrew {amountToWithdraw} from your {validAccounts[accountChoice]} account!\n");
                                    break;
                            }
                        }
                        break;
                    // Deposit balance
                    case 2:
                        {
                            int accountChoice = UI.PromptFromList("which account", validAccounts);
                            int amountToDeposit = UI.PromptForInteger("how much to deposit");

                            var error = DepositTo(accountChoice, amountToDeposit);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot deposit a negative amount!\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully deposited {amountToDeposit} to your {validAccounts[accountChoice]} account!\n");
                                    break;
                            }
                            break;
                        }
                    // Transfer balance
                    case 3:
                        {
                            int accountChoice = UI.PromptFromList("from which account", validAccounts);
                            int amountToTransfer = UI.PromptForInteger("how much to transfer");

                            var error = TransferFrom(accountChoice, amountToTransfer);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot transfer a negative amount!\n");
                                    break;
                                case AccountError.OVERFLOW:
                                    Console.WriteLine("You don't have that much to transfer!\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully trasnfered {amountToTransfer} from your {validAccounts[accountChoice]} account!\n");
                                    break;
                            }
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

        private void DisplayBalances()
        {
            UI.WriteList("balances", new List<string> {
                    $"Checkings: {SumTransactionsFor(AccountType.CHECKINGS)}",
                    $"Savings: {SumTransactionsFor(AccountType.SAVINGS)}\n"});
        }

        private int DepositTo(int accountType, int amount)
        {
            if (amount < 0) return AccountError.UNDERFLOW;
            CommitTransaction(new Transaction(ConnectedAccountID, accountType, AccountAction.DEPOSIT, amount));
            return AccountError.NONE;
        }


        private int WithdrawFrom(int accountType, int amount)
        {

            if (amount < 0) return AccountError.UNDERFLOW;
            if (amount > SumTransactionsFor(accountType)) return AccountError.OVERFLOW;

            CommitTransaction(new Transaction(ConnectedAccountID, accountType, AccountAction.WITHDRAWAL, amount));

            return AccountError.NONE;
        }

        private int TransferFrom(int accountType, int amount)
        {
            int transferType = -1;

            switch (accountType)
            {
                case AccountType.CHECKINGS:
                    transferType = AccountType.SAVINGS;
                    break;
                case AccountType.SAVINGS:
                    transferType = AccountType.CHECKINGS;
                    break;
            }

            int error = WithdrawFrom(accountType, amount);

            if (error == AccountError.NONE)
            {
                CommitTransaction(new Transaction(ConnectedAccountID, transferType, AccountAction.DEPOSIT, amount));
            }

            return error;
        }

        private bool TryLoadAccountTransactions(out List<Transaction> transactions)
        {
            if (File.Exists("transactions.csv"))
            {
                var reader = new StreamReader("transactions.csv");
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

                transactions = csvReader.GetRecords<Transaction>()
                    .Where(account => account.AccountID == ConnectedAccountID)
                    .ToList();

                reader.Close();

                return transactions.Count != 0;
            }
            else
            {
                var writer = new StreamWriter("transactions.csv");
                var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

                csvWriter.WriteHeader<Transaction>();
                csvWriter.NextRecord();
                csvWriter.Flush();
                writer.Close();
            }

            transactions = new List<Transaction>();

            return false;
        }

        private void CommitTransaction(Transaction transaction)
        {
            var writer = new StreamWriter("transactions.csv", append: true);
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csvWriter.WriteRecord(transaction);
            csvWriter.NextRecord();
            ConnectedAccountTransactions.Add(transaction);

            writer.Close();
        }

        private int SumTransactionsFor(int accountType)
        {
            int balance = 0;

            foreach (var transact in ConnectedAccountTransactions.Where(transact => transact.AccountType == accountType))
            {
                switch (transact.AccountAction)
                {
                    case AccountAction.DEPOSIT:
                        balance += transact.AccountDelta;
                        break;
                    case AccountAction.WITHDRAWAL:
                        balance -= transact.AccountDelta;
                        break;
                }
            }

            return balance;
        }
    }
}

