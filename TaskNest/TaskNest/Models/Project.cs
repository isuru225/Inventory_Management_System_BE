using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Models
{
    public class Project
    {
        [BsonId]
        public string Id { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public List<ProjectTask> ProjectTasks { get; set; } 
        public List<string> ApplicationUsers { get; set; }
        public List<string> Technologies { get; set; }
    }
}
