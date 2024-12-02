using MongoDB.Driver;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IMongoDbService
    {
        IMongoCollection<Project> Projects { get; }
        IMongoCollection<ProjectTask> ProjectTasks { get; }
        IMongoCollection<ApplicationUser> applicationUsers { get; }
        IMongoCollection<RawDrug> RawDrugs { get; }
    }
}
