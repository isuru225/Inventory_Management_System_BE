namespace TaskNest.Frontend.Models
{
    public class ForgetPasswordRequest
    {
        public string Email { get; set; }
        public string ClientURI { get; set; } = string.Empty;
    }
}
