namespace SuncoastBank
{
    public class Transaction
    {
        public int AccountID { get; }
        public int AccountType { get; }
        public int AccountDelta { get; }
        public int AccountAction { get; }

        public Transaction(int AccountID, int AccountType, int AccountDelta, int AccountAction)
        {
            this.AccountID = AccountID;
            this.AccountType = AccountType;
            this.AccountAction = AccountAction;
            this.AccountDelta = AccountDelta;
        }
    }
}
