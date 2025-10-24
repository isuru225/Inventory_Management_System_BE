using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class HistoryInfo
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Item name is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Item name must be alphanumeric.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Item name must be at most 20 characters.")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Initial amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Initial amount must be at least 0.")]
        public double InitialAmount { get; set; }

        [Required(ErrorMessage = "Current amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Current amount must be at least 0.")]
        public double CurrentAmount { get; set; }

        [Required(ErrorMessage = "Adjusted amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Adjusted amount must be at least 0.")]
        public double AdjustedAmount { get; set; }
        [Required(ErrorMessage = "Adjustment type is required.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Adjustment type must contain only numbers.")]
        public string AdjustmentType { get; set; }
        [Required(ErrorMessage = "Storekeeper name is required.")]
        [RegularExpression("^[a-zA-Z].*$", ErrorMessage = "Storekeeper name must be alphanumeric.")]
        public string StoreKeeper { get; set; }
        [Required(ErrorMessage = "Measurement unit is required.")]
        public string MeasurementUnit { get; set; }
        [Required(ErrorMessage = "Transaction time is required.")]
        public DateTime Time { get; set; }
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Reason must be at most 50 characters.")]
        public string? Reason { get; set; } 
    }
}
