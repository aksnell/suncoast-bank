namespace SuncoastBank
{
    public class Transaction
    {
        public string Name            { get; set; }
        public string AccountType     { get; set; }
        public string TransactionType { get; set; }
        public int AccountDelta       { get; set; }
    }
}
