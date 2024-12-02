using MongoDB.Bson.Serialization.Attributes;
using TaskNest.Frontend.Models;

namespace TaskNest.Models
{
    public class ProjectTask
    {
        [BsonId]
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string TaskTitle { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int PriorityLevel { get; set; }
        public EmployeeInfo CreatedEmployeeInfo { get; set; }
        public List<EmployeeInfo> AssignedEmployeeInfos { get; set; }
    }
}
