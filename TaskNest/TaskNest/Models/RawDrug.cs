using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class RawDrug
    {
        [BsonId]
        public string Id { get; set; }
        public string ItemName { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Category { get; set; }
        public double Amount { get; set; }
        public string MeasurementUnit { get; set; }
    }
}
