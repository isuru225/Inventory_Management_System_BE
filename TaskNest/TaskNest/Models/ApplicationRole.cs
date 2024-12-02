using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using AspNetCore.Identity.MongoDbCore.Models;

namespace TaskNest.Models
{
    public class ApplicationRole : MongoIdentityRole<string>
    {
        
    }
}
