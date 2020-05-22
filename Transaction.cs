namespace SuncoastBank
{
    public struct Transaction
    {
        public string Name            { get; set; }
        public string AccountType     { get; set; }
        public int AccountDelta       { get; set; }
    }
}
