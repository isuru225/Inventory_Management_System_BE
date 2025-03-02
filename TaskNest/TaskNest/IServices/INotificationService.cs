namespace TaskNest.IServices
{
    public interface INotificationService
    {
        public Task<List<string>> GetAllNotification();
    }
}
