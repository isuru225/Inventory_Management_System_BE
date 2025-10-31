using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class GeneralStoreItemInfo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        [RegularExpression("^[a-zA-Z0-9].*$", ErrorMessage = "Item name must be alphanumeric.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Item name must be at most 20 characters.")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(typeof(decimal), "1", "79228162514264337593543950335", ErrorMessage = "Amount must be at least 1.")]
        public decimal Amount { get; set; }
    }
}
