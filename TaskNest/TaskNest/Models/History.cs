using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class History
    {
        [BsonId]
        public string Id { get; set; }
        public string ItemName { get; set; }
        public decimal InitialAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public string AdjustmentType { get; set; }
        public string StoreKeeper { get; set; }
        public string MeasurementUnit { get; set; }
        public DateTime Time { get; set; }
        public string? Reason { get; set; }
    }
}
