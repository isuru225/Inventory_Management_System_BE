using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class InventoryUpdate
    {
        [Required(ErrorMessage = "Balance is required.")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Balance must be at least 0.")]
        public decimal Balance { get; set; }

        [Required(ErrorMessage = "Initial amount is required.")]
        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Initial amount must be at least 0.")]
        public decimal InitialAmount { get; set; }
        [Required(ErrorMessage = "Item name is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Item name must be alphanumeric.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Item name must be at most 20 characters.")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Author name must be alphanumeric.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Adjustment type is required.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Adjustment type must contain only numbers.")]
        public string AdjustmentType { get; set; }

        [Required(ErrorMessage = "Adjusted amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Adjusted amount must be at least 0.")]
        public decimal AmountAdjusted { get; set; }
        [Required(ErrorMessage = "Measurement unit is required.")]
        public string MeasurementUnit { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Reason must be alphanumeric.")]
        [StringLength(50, ErrorMessage = "Reason must be at most 50 characters.")]
        public string? Reason { get; set; }
    }
}
