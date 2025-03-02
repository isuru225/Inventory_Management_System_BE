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
        public NotificationService(IMongoDbService mongoDbService, ILogger<HomeService> logger, IRawDrugService rawDrugService)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _rawDrugService = rawDrugService;
        }

        public async Task<List<string>> GetAllNotification()
        {
            try
            {
                List<RawDrugInfo> rawDrugs = new List<RawDrugInfo>();
                //list of raw drug names which are having low inventory
                List<string> rawDrugNames = new List<string>();
                rawDrugs = await _rawDrugService.GetAllRawDrugs();

                foreach (var rawDrug in rawDrugs) 
                {
                    if ((rawDrug.Amount - rawDrug.ReorderPoint) <= 0) 
                    {
                        rawDrugNames.Add(rawDrug.ItemName);
                    }
                }

                return rawDrugNames;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting all raw drugs");
                throw ex;
            }
        }
    }
}
