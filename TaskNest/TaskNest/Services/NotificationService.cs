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
        private IRawDrugService _rawDrugService;
        private IFinishedDrugService _finishedDrugService;
        public NotificationService(IMongoDbService mongoDbService, ILogger<HomeService> logger, IRawDrugService rawDrugService, IFinishedDrugService finishedDrugService)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _rawDrugService = rawDrugService;
            _finishedDrugService = finishedDrugService;
        }

        public async Task<List<Notification>> GetAllNotification()
        {
            try
            {   
                List<RawDrugInfo> rawDrugs = new List<RawDrugInfo>();
                List<FinishedDrugInfo> finishedDrugs = new List<FinishedDrugInfo>();

                List<Notification> notifingDrugItems = new List<Notification>();
                //list of raw drug names which are having low inventory
                rawDrugs = await _rawDrugService.GetAllRawDrugs();
                finishedDrugs = await _finishedDrugService.GetAllFinishedDrugs();


                Notification rawDrugNotification = new Notification();
                Notification finishedDrugNotification = new Notification();
                Notification GeneralStoreItemNotification = new Notification();

                //Add notification items to rawDrugNotification
                rawDrugNotification.ItemType = "raw_drug";
                // Initialize the item list
                rawDrugNotification.ItemList = new List<string>();
                foreach (var rawDrug in rawDrugs) 
                {
                    if ((rawDrug.Amount - rawDrug.ReorderPoint) <= 0) 
                    {
                        rawDrugNotification.ItemList.Add(rawDrug.ItemName);
                    }
                }
                //Add notification items to finishedDrugNotification
                finishedDrugNotification.ItemType = "finished_drug";
                // Initialize the item list
                finishedDrugNotification.ItemList = new List<string>();
                foreach (var finishedDrug in finishedDrugs) 
                {
                    if ((finishedDrug.Amount - finishedDrug.ReorderPoint) <= 0) 
                    {
                        finishedDrugNotification.ItemList.Add(finishedDrug.ItemName);
                    }
                }

                notifingDrugItems.Add(rawDrugNotification);
                notifingDrugItems.Add(finishedDrugNotification);


                return notifingDrugItems;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting all drug items or general store items");
                throw ex;
            }
        }
    }
}
