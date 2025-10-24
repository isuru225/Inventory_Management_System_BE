using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class ResetPassword
    {
        [Required(ErrorMessage = "Email address is required!")]
        [EmailAddress(ErrorMessage = "UserName should be an email!")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Password Reset token is required!")]
        public string PasswordResetToken { get; set; }
        [Required(ErrorMessage = "New Password is required.")]
        [RegularExpression(
        @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!#%$]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase, one uppercase, one digit, and one special character (!, #, %, $)."
    )]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Password confirmation is required!")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation new password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmedNewPassword { get; set;}
    }
}
