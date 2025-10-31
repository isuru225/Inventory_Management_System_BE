using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class FinishedDrug
    {
        [BsonId]
        public string Id { get; set; }
        public string ItemName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal ReorderPoint { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
