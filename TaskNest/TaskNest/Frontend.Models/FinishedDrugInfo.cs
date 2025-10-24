using System.ComponentModel.DataAnnotations;

namespace TaskNest.Frontend.Models
{
    public class FinishedDrugInfo
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
        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be at least 1.")]
        public double Amount { get; set; }
        [Required(ErrorMessage = "Reorder point is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Reorder point must be at least 1.")]
        public double ReorderPoint { get; set; }
        [Required(ErrorMessage = "Measurement unit is required.")]
        public string MeasurementUnit { get; set; }
    }
}
