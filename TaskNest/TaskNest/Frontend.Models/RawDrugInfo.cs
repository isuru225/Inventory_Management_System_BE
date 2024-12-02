namespace TaskNest.Frontend.Models
{
    public class RawDrugInfo
    {
        public string Id { get; set; }
        public string ItemName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Category { get; set; }
        public int Amount { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
