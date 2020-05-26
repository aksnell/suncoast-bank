namespace SuncoastBank
{
    public struct AccountType
    {
        public const int
            CHECKINGS = 0,
            SAVINGS = 1;
    }

    public struct AccountError
    {
        public const int
            NONE = 0,
            UNDERDRAFT = 1,
            OVERDRAFT = 2;
    }
}
