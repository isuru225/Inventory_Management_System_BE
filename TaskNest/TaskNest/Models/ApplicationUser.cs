using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;
using TaskNest.Frontend.Models;

namespace TaskNest.Models
{
    public class ApplicationUser : MongoIdentityUser<string>
    {
        [BsonElement("phone_number")]
        public override string PhoneNumber { get; set; }  // Override from base class

        [BsonElement("phone_number_confirmed")]
        public override bool PhoneNumberConfirmed { get; set; }  // Properly named as per IdentityUser

        // Additional custom fields
        [BsonElement("full_name")]
        public string FullName { get; set; }
        public List<string> ProjectIds { get; set; }
    }
}
