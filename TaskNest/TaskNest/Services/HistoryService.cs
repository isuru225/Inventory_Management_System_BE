using MongoDB.Bson;
using MongoDB.Driver;
using TaskNest.Custom.Exceptions;
using TaskNest.Enum;
using TaskNest.Frontend.Models;
using TaskNest.Helper;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        public HistoryService(IMongoDbService mongoDbService, ILogger<RawDrugService> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<Object> AddHistoryRecord(HistoryInfo historyInfo) 
        {
            var (isValid, errors) = ValidationHelper.ValidateObject(historyInfo);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            try 
            {
                History history = new History();
                history.Id = ObjectId.GenerateNewId().ToString();
                history.ItemName = historyInfo.ItemName;
                history.InitialAmount = historyInfo.InitialAmount;
                history.CurrentAmount = historyInfo.CurrentAmount;
                history.AdjustedAmount = historyInfo.AdjustedAmount;
                history.AdjustmentType = historyInfo.AdjustmentType;
                history.StoreKeeper = historyInfo.StoreKeeper;
                history.MeasurementUnit = historyInfo.MeasurementUnit;
                history.Time = historyInfo.Time;
                history.Reason = historyInfo.Reason;

                await _mongoDbService.History.InsertOneAsync(history);

                return new
                {
                    message = "New history record is successfully added",
                    isSuccessful = true
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while adding history record");
                throw;
            }
        }

        public async Task<List<HistoryInfo>> GetAllHistoryRecord()
        {
            try
            {


                var filter = Builders<History>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.History.FindAsync(filter);

                var historyRecordResultList = result.ToList();

                List<HistoryInfo> historyRecordList = new List<HistoryInfo>();

                foreach (var historyRecord in historyRecordResultList) 
                {
                    HistoryInfo historyInfo = new HistoryInfo();
                    historyInfo.Id = historyRecord.Id;
                    historyInfo.ItemName = historyRecord.ItemName;
                    historyInfo.InitialAmount = historyRecord.InitialAmount;
                    historyInfo.CurrentAmount = historyRecord.CurrentAmount;
                    historyInfo.AdjustedAmount = historyRecord.AdjustedAmount;
                    historyInfo.AdjustmentType = historyRecord.AdjustmentType;
                    historyInfo.StoreKeeper = historyRecord.StoreKeeper;
                    historyInfo.MeasurementUnit = historyRecord.MeasurementUnit;
                    historyInfo.Time = historyRecord.Time;
                    historyInfo.Reason = historyRecord.Reason;

                    historyRecordList.Add(historyInfo);
                }

               return historyRecordList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while adding history record");
                throw;
            }
        }


        public async Task<object> DeleteHistoryRecord(string historyRecordId)
        {
            try
            {
               
                List<int> numberList = new List<int> { 1,2,3,4,5 };

                foreach (var number in numberList) 
                {
                    Console.WriteLine(number);
                }
                IEnumerable<int> numbers = numberList;

                while (numbers.GetEnumerator().MoveNext()) 
                {
                    Console.WriteLine(numbers.GetEnumerator().Current);
                }

                var filter = Builders<History>.Filter.Eq(doc => doc.Id, historyRecordId);
                var result = await _mongoDbService.History.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    return new
                    {
                        Message = "Record deleted successfully.",
                        IsSuccessful = true
                    };
                }
                else
                {
                    return new
                    {
                        Message = "No record found with the given Id.",
                        IsSuccessful = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the history record in the DB.");
                throw;
            }
        }
    }
}
