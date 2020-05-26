namespace SuncoastBank
{

    public class Transaction
    {
        public int AccountID { get; }
        public int AccountType { get; }

        private int _accountDelta;
        public virtual int AccountDelta {
            get { return _accountDelta; }
            set { _accountDelta = value; }
        }

        public Transaction(int AccountID, int AccountType, int AccountDelta)
        {
            this.AccountID = AccountID;
            this.AccountType = AccountType;
            this.AccountDelta = AccountDelta;
        }

        public virtual int GetDelta()
        {
            return AccountDelta;
        }
    }

    public class WithdrawTransaction : Transaction
    {
        private int _accountDelta;
        public override int AccountDelta
        {
            get { return _accountDelta; }
            set { _accountDelta = -1 * value; }
        }

        public WithdrawTransaction(int AccountID, int AccountType, int AccountDelta) : base(AccountID, AccountType, AccountDelta) {}
    }

    public class DepositTransaction : Transaction
    {
        public DepositTransaction(int AccountID, int AccountType, int AccountDelta) : base(AccountID, AccountType, AccountDelta) {}
    }
}
