using TaskNest.Frontend.Models;

namespace TaskNest.IServices
{
    public interface IAdminService
    {
        public Task<Object> AddProjects(ProjectInfo projectInfo);
    }
}
