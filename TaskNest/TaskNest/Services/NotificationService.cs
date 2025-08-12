using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using TaskNest.Frontend.Models;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class NotificationService : INotificationService
    {
        private IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        public NotificationService(IMongoDbService mongoDbService, ILogger<HomeService> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<List<Notification>> GetAllNotification()
        {
            try
            {
                var filter = Builders<Notification>.Filter.Empty;
                var result = await _mongoDbService.Notifications.FindAsync(filter);

                //Get all available notifications
                var allExistingNotifications = await result.ToListAsync();

                return allExistingNotifications;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting all notifications");
                throw ex;
            }
        }

        public async Task<Object> AddNotification(NotificationInfo notificationInfo) 
        {
            try
            {
                //var filter = builders<notification>.filter.empty;
                //var result = await _mongodbservice.notifications.findasync(filter);

                //var allexisitingnotifications = await result.tolistasync();
                //allexisitingnotifications.find(x => x.)
                Notification notification = new Notification();
                notification.NotificationType = notificationInfo.NotificationType;
                notification.ItemType = notificationInfo.ItemType;
                notification.ItemName = notificationInfo.ItemName;
                notification.CreatedAt = notificationInfo.CreatedAt;
                notification.Id = ObjectId.GenerateNewId().ToString();

                await _mongoDbService.Notifications.InsertOneAsync(notification);
                return new
                {
                    message = "New notification is successfully added",
                    isSuccessful = true
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while writting data into raw drug collection");
                throw ex;
            }
        }

        public async Task<List<Notification>> UpdateAvailableNotificationAsMarked() 
        {
            try
            {
                var filter = Builders<Notification>.Filter.Eq(x => x.IsRead, false);
                var update = Builders<Notification>.Update.Set(x => x.IsRead, true);

                var result = await _mongoDbService.Notifications.UpdateManyAsync(filter,update);

                try
                {

                    var filterNotification = Builders<Notification>.Filter.Empty;
                    var resultNotification = await _mongoDbService.Notifications.FindAsync(filterNotification);

                    var updatedNotifications = await resultNotification.ToListAsync();

                    return updatedNotifications;
                }
                catch (Exception ex) 
                {
                    throw ex;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while updating isRead property of the notification document");
                throw ex;
            }
        }
    }
}
