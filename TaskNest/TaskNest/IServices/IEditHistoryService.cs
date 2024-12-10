using TaskNest.Frontend.Models;

namespace TaskNest.IServices
{
    public interface IEditHistoryService
    {
        public Task<Object> AddEditHistory(EditHistoryInfo editHistoryInfo);
        public Task<List<EditHistoryInfo>> GetAllEditHistory();
    }
}
