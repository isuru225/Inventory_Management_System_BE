using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IHomeService
    {
        public Task<List<Project>> GetAllProjects();
    }
}
