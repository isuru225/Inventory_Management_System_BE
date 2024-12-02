using TaskNest.Frontend.Models;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IProjectService
    {
        public Task<object> CreateTask(TaskInfo taskInfo);
        public Task<List<ProjectTask>> GetProjectSpecificTask(string projectId);
    }
}
