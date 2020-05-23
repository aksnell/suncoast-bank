namespace SuncoastBank
{
    public class Transaction
    {
        public int AccountID { get; set; }
        public int AccountType { get; set; }
        public int AccountDelta { get; set; }
        public int AccountAction { get; set; }

        public Transaction() { }
        public Transaction(int accountID, int accountType, int accountAction, int accountDelta)
        {
            AccountID = accountID;
            AccountType = accountType;
            AccountAction = accountAction;
            AccountDelta = accountDelta;
        }
    }
}
