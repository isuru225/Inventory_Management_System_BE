namespace TaskNest.Frontend.Models
{
    public class InventoryUpdate
    {
        public double Balance { get; set; }
        public double InitialAmount { get; set; }
        public string ItemName { get; set; }
        public string Author { get; set; }
        public string AdjustmentType { get; set; }
        public double AmountAdjusted { get; set; }
        public string MeasurementUnit { get; set; }
        public string Reason { get; set; }
    }
}
