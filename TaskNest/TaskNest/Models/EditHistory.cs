using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class EditHistory
    {
        [BsonId]
        public string TransactionId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ItemName { get; set; }
        public int ChangedAmount { get; set; }
        public bool IsReducedAmount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
