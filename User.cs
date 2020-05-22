namespace SuncoastBank
{
    public struct User
    {
        public string Name  { get; set; }
        public int Password { get; set; }
        public int Salt     { get; set; }
    }
}
