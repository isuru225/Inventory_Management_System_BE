using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class CreateRole
    {
        [Required(ErrorMessage = "Role name is required!")]
        public string RoleName { get; set; }
    }
}
