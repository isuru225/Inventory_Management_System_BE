using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class UserRegisterInfo
    {
        [Required(ErrorMessage ="First Name is required!")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required!")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mobile Number is required!")]
        [Phone]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(
        @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!#%$]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and contain at least one lowercase, one uppercase, one digit, and one special character (!, #, %, $)."
        )]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirmation Password is required!")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmedPassword { get; set; }
        [Required(ErrorMessage = "User role is required!")]
        public string Role { get; set; }
    }
}
