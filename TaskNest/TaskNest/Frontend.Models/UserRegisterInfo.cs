using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class UserRegisterInfo
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string MobileNumber { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmedPassword { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
