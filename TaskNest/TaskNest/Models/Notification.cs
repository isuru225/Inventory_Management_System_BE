using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class Notification
    {
        [BsonId]
        public string Id { get; set; }
        public int ItemType { get; set; }
        public int NotificationType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ItemName { get; set; }
        public Boolean IsRead { get; set; } = false;
    }
}
