using System.ComponentModel.DataAnnotations;

namespace TaskNest.FrontendModels
{
    public class UserLogin
    {
        [Required(ErrorMessage = "UserName is required!")]
        [EmailAddress(ErrorMessage = "UserName should be an email!")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
        @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!#%$]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase, one uppercase, one digit, and one special character (!, #, %, $)."
        )]
        public string Password { get; set; }
    }
}
