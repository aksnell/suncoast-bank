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
                            int amountToWithdraw = ConvertCents(UI.PromptForFloat("withdrawal amount"));

                            var error = WithdrawFrom(accountChoice, amountToWithdraw);

                            switch (error)
                            {
                                case AccountError.UNDERDRAFT:
                                    Console.WriteLine("You cannot withdraw less than a penny!.\n");
                                    break;
                                case AccountError.OVERDRAFT:
                                    Console.WriteLine("You don't have that much to withdraw.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully withdrew {UI.FormatCurrency(amountToWithdraw)} from your {validAccounts[accountChoice]} account.\n");
                                    break;
                            }
                            break;
                        }
                    // Deposit balance
                    case 2:
                        {
                            int accountChoice = UI.PromptFromList("to which account", validAccounts);
                            int amountToDeposit = ConvertCents(UI.PromptForFloat("deposit amount"));

                            var error = DepositTo(accountChoice, amountToDeposit);

                            switch (error)
                            {
                                case AccountError.UNDERDRAFT:
                                    Console.WriteLine("You cannot deposit a less than a penny!.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully deposited {UI.FormatCurrency(amountToDeposit)} to your {validAccounts[accountChoice]} account.\n");
                                    break;
                            }
                            break;
                        }

                    // Transfer balance
                    case 3:
                        {
                            int accountChoice = UI.PromptFromList("from which account", validAccounts);
                            int amountToTransfer = ConvertCents(UI.PromptForFloat("transfer amount"));

                            var error = TransferFrom(accountChoice, amountToTransfer);

                            switch (error)
                            {
                                case AccountError.UNDERDRAFT:
                                    Console.WriteLine("You cannot transfer less than a penny!.\n");
                                    break;
                                case AccountError.OVERDRAFT:
                                    Console.WriteLine("You don't have that much to transfer.\n");
                                    break;
                                case AccountError.NONE:
                                    Console.WriteLine($"Succesfully transferred {UI.FormatCurrency(amountToTransfer)} from your {validAccounts[accountChoice]} account.\n");
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
                        $"Checkings: {UI.FormatCurrency(SumTransactionsFor(AccountType.CHECKINGS))}",
                        $"Savings: {UI.FormatCurrency(SumTransactionsFor(AccountType.SAVINGS))}\n"
                    }
            );
        }

        private int DepositTo(int depositAccount, int despoitAmount)
        {
            if (despoitAmount < 0) return AccountError.UNDERDRAFT;

            CommitTransaction(new DepositTransaction(AccountID, depositAccount, despoitAmount));

            return AccountError.NONE;
        }


        private int WithdrawFrom(int withdrawAccount, int withdrawAmount)
        {
            if (withdrawAmount < 0) return AccountError.UNDERDRAFT;
            if (withdrawAmount > SumTransactionsFor(withdrawAccount)) return AccountError.OVERDRAFT;

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
                using (var reader = new StreamReader("transactions.csv"))
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    transactions = csvReader.GetRecords<Transaction>()
                        .Where(account => account.AccountID == AccountID)
                        .ToList();
                }

                return transactions.Count == 0;
            }

            transactions = new List<Transaction>();

            return false;
        }

        private void CommitTransaction(Transaction transaction)
        {
            using (var writer = new StreamWriter("transactions.csv", true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (new FileInfo("transactions.csv").Length == 0)
                {
                    csv.WriteHeader<Transaction>();
                }
                csv.NextRecord();
                csv.WriteRecord<Transaction>(transaction);
            }

            AccountTransactions.Add(transaction);
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

        private int ConvertCents(float dollarAmount)
        {
            return (int)(dollarAmount * 100);
        }
    }
}

