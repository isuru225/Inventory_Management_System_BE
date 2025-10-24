using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class ForgetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Website URL is required.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string ClientURI { get; set; } = string.Empty;
    }
}
