namespace Model.Helper
{
    public class JWT
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public double ExpiresMinutes { get; set; }

    }
}
