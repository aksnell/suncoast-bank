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

        private int AccountID;
        private List<Transaction> AccountTransactions;
        private FrontEnd UI;

        public Bank(int accountID)
        {
            AccountID = accountID;
            UI = new FrontEnd();
        }


        public void Connect()
        {
            bool isConnected = true;

            List<string> validAccounts = new List<string>
            {
                "Checkings",
                "Savings"
            };

            var accounTransactions = new List<Transaction>();


            if (TryLoadAccountTransactions(out AccountTransactions))
            {
                DisplayBalances();
            }

            while (isConnected)
            {
                int userInput = UI.PromptFromList("choose one of the following", new List<string> 
                        {
                            "View Balance",
                            "Withdraw",
                            "Deposit",
                            "Transfer",
                            "Quit"
                        });

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
                            int accountChoice = UI.PromptFromList("from which account", validAccounts);
                            int amountToWithdraw = UI.PromptForInteger("withdrawal amount");

                            var error = WithdrawFrom(accountChoice, amountToWithdraw);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot withdraw a negative amount.\n");
                                    break;
                                case AccountError.OVERFLOW:
                                    Console.WriteLine("You don't have that much to withdraw.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully withdrew {amountToWithdraw} from your {validAccounts[accountChoice]} account.\n");
                                    break;
                            }
                            break;
                        }
                    // Deposit balance
                    case 2:
                        {
                            int accountChoice = UI.PromptFromList("to which account", validAccounts);
                            int amountToDeposit = UI.PromptForInteger("deposit amount");

                            var error = DepositTo(accountChoice, amountToDeposit);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot deposit a negative amount.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully deposited {amountToDeposit} to your {validAccounts[accountChoice]} account.\n");
                                    break;
                            }
                            break;
                        }

                    // Transfer balance
                    case 3:
                        {
                            int accountChoice = UI.PromptFromList("from which account", validAccounts);
                            int amountToTransfer = UI.PromptForInteger("transfer amount");

                            var error = TransferFrom(accountChoice, amountToTransfer);

                            switch (error)
                            {
                                case AccountError.UNDERFLOW:
                                    Console.WriteLine("You cannot transfer a negative amount.\n");
                                    break;
                                case AccountError.OVERFLOW:
                                    Console.WriteLine("You don't have that much to transfer.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully trasnfered {amountToTransfer} from your {validAccounts[accountChoice]} account.\n");
                                    break;
                            }
                            break;
                        }
                    // Quit
                    case 4:
                        {
                            Console.WriteLine("Goodbye.");
                            isConnected = false;
                            break;
                        }
                }
            }
        }

        private void DisplayBalances()
        {
            UI.WriteList("balances", new List<string> 
                    {
                        $"Checkings: {SumTransactionsFor(AccountType.CHECKINGS)}",
                        $"Savings: {SumTransactionsFor(AccountType.SAVINGS)}\n"
                    }
            );
        }

        private int DepositTo(int depositAccount, int despoitAmount)
        {
            if (despoitAmount < 0) return AccountError.UNDERFLOW;

            CommitTransaction(new DepositTransaction(AccountID, depositAccount, despoitAmount));

            return AccountError.NONE;
        }


        private int WithdrawFrom(int withdrawAccount, int withdrawAmount)
        {
            if (withdrawAmount < 0) return AccountError.UNDERFLOW;
            if (withdrawAmount > SumTransactionsFor(withdrawAccount)) return AccountError.OVERFLOW;

            CommitTransaction(new WithdrawTransaction(AccountID, withdrawAccount, withdrawAmount));

            return AccountError.NONE;
        }

        private int TransferFrom(int transferAccount, int transferAmount)
        {
            int transferToAccount = transferAccount ^ 1;

            int result = WithdrawFrom(transferAccount, transferAmount);
            if (result == AccountError.NONE)
            {
                CommitTransaction(new DepositTransaction(AccountID, transferToAccount, transferAmount));
            }

            return result;
        }

        private bool TryLoadAccountTransactions(out List<Transaction> transactions)
        {
            if (File.Exists("transactions.csv"))
            {
                var reader = new StreamReader("transactions.csv");
                var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

                transactions = csvReader.GetRecords<Transaction>()
                    .Where(account => account.AccountID == AccountID)
                    .ToList();

                return transactions.Count == 0;
            }

            transactions = new List<Transaction>();

            return false;
        }

        private void CommitTransaction(Transaction transaction)
        {
            var writer = new StreamWriter("transactions.csv", append: true);
            var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if (!File.Exists("transactions.csv"))
            {
                csvWriter.NextRecord();
                csvWriter.WriteHeader<Transaction>();
                csvWriter.Flush();
            }

            csvWriter.NextRecord();
            csvWriter.WriteRecord(transaction);
            csvWriter.NextRecord();

            AccountTransactions.Add(transaction);

            writer.Close();
        }

        private int SumTransactionsFor(int accountType)
        {
            int balance = 0;

            foreach (var transaction in AccountTransactions.Where(transact => transact.AccountType == accountType))
            {
                balance += transaction.AccountDelta;
            }
            return balance;
        }
    }
}

