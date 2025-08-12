using TaskNest.Frontend.Models;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface INotificationService
    {
        public Task<List<Notification>> GetAllNotification();
        public Task<Object> AddNotification(NotificationInfo notificationInfo);
        public Task<List<Notification>> UpdateAvailableNotificationAsMarked();
    }
}
