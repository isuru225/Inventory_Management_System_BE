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
        IMongoCollection<History> History { get; }
        IMongoCollection<ApplicationRole> applicationRoles { get; }
        IMongoCollection<FinishedDrug> FinishedDrugs { get; }
        IMongoCollection<GeneralStoreItem> GeneralStoreItems {  get; }
        IMongoCollection<Notification> Notifications { get; }
    }
}
