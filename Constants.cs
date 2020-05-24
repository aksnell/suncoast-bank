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
}
