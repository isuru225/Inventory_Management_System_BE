using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class ResetPassword
    {
        public string Email { get; set; }
        public string PasswordResetToken { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation new password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set;}
    }
}
