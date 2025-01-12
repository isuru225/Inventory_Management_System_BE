using TaskNest.Frontend.Models;

namespace TaskNest.IServices
{
    public interface IHistoryService
    {
        public Task<Object> AddHistoryRecord(HistoryInfo historyInfo);
        public Task<List<HistoryInfo>> GetAllHistoryRecord();
        public Task<object> DeleteHistoryRecord(string historyRecordId);
    }
}
