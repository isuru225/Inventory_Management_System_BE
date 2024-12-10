using MongoDB.Driver;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoDatabase _database;
        //var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");
        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("InventoryManagementDb");
        }
    
        // Collection for Project
        public IMongoCollection<Project> Projects =>
            _database.GetCollection<Project>("Projects");

        public IMongoCollection<ProjectTask> ProjectTasks =>
            _database.GetCollection<ProjectTask>("ProjectTasks");

        public IMongoCollection<ApplicationUser> applicationUsers =>
            _database.GetCollection<ApplicationUser>("applicationUsers");

        public IMongoCollection<RawDrug> RawDrugs =>
            _database.GetCollection<RawDrug>("RawDrugs");
        public IMongoCollection<EditHistory> EditHistory =>
            _database.GetCollection<EditHistory>("EditHistory");
    }
}
