namespace TaskNest.Frontend.Models
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
        public string Message { get; set; }
    }
}
    