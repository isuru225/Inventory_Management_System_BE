using TaskNest.Frontend.Models;

namespace TaskNest.IServices
{
    public interface INotificationService
    {
        public Task<List<Notification>> GetAllNotification();
    }
}
