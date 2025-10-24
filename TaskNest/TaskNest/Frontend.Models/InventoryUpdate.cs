using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class InventoryUpdate
    {
        [Required(ErrorMessage = "Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance must be at least 0.")]
        public double Balance { get; set; }

        [Required(ErrorMessage = "Initial amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Initial amount must be at least 0.")]
        public double InitialAmount { get; set; }
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
        public double AmountAdjusted { get; set; }
        [Required(ErrorMessage = "Measurement unit is required.")]
        public string MeasurementUnit { get; set; }

        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Comment must be alphanumeric.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Comment must be at most 50 characters.")]
        public string Reason { get; set; }
    }
}
