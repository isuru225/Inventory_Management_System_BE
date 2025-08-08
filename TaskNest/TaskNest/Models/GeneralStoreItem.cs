using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class GeneralStoreItem
    {
        [BsonId]
        public string Id { get; set; }
        public string ItemName { get; set; }
        public double Amount { get; set; }
    }
}
