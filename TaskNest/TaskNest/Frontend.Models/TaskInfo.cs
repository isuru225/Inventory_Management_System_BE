using MongoDB.Bson.Serialization.Attributes;

namespace TaskNest.Frontend.Models
{
    public class TaskInfo
    {     
        public string ProjectId { get; set; }
        public string TaskTitle { get; set; }
        public string CreatedEmployeeId { get; set; }
        public List<string> AssignedEmployeeIds { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int Status { get; set; }
        public int PriorityLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
