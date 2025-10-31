using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class RawDrugInfo
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Item name is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Item name must be alphanumeric.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Item name must be at most 20 characters.")]
        public string ItemName { get; set; }
        [Required(ErrorMessage = "Expiration date is required.")]
        public DateTime ExpirationDate { get; set; }
        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }
        [Required(ErrorMessage = "Measurement unit is required.")]
        public string MeasurementUnit { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(typeof(decimal), "0.000001", "79228162514264337593543950335", ErrorMessage = "Reorder point must be a positive number.")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "Reorder point is required.")]
        [Range(typeof(decimal), "0.000001", "79228162514264337593543950335", ErrorMessage = "Reorder point must be a positive number.")]
        public decimal ReorderPoint { get; set; }
    }
}
