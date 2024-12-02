using System.ComponentModel.DataAnnotations;

namespace TaskNest.FrontendModels
{
    public class UserLogin
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
