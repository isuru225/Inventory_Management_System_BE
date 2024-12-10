using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using TaskNest.Frontend.Models;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditHistoryService : IEditHistoryService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        public EditHistoryService(IMongoDbService mongoDbService, ILogger<EditHistoryService> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<Object> AddEditHistory(EditHistoryInfo editHistoryInfo)
        {
                    try
                    {
                // Get current Sri Lankan time
                DateTime serverTime = DateTime.Now; // gives you current Time in server timeZone
                DateTime utcTime = serverTime.ToUniversalTime(); // convert it to Utc using timezone setting of server computer 
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time"));


                //create new editHistory instance
                EditHistory editHistory = new EditHistory();
                        editHistory.UserName = editHistoryInfo.UserName;
                        editHistory.ItemName = editHistoryInfo.ItemName;
                        editHistory.ChangedAmount = editHistoryInfo.ChangedAmount;
                        editHistory.TransactionDate = localTime;
                        if (editHistoryInfo.IsReducedAmount == true)
                        {
                          editHistory.IsReducedAmount = true;
                }
                        else
                        {
                          editHistory.IsReducedAmount = false;
                }
                        editHistory.UserId = editHistoryInfo.UserId;
                        editHistory.TransactionId = ObjectId.GenerateNewId().ToString();

                        await _mongoDbService.EditHistory.InsertOneAsync(editHistory);
                        return new
                        {
                            message = "New edit history transaction is successfully added",
                            isSuccessful = true
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while writting data into edit history collection");
                        throw ex;
                    }
        }

        public async Task<List<EditHistoryInfo>> GetAllEditHistory()
        {
            try
            {
                var filter = Builders<EditHistory>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.EditHistory.FindAsync(filter);  // Get async cursor

                var allExistingEditHistory = await result.ToListAsync();
                List<EditHistoryInfo> editHistoryList = new List<EditHistoryInfo>();

                foreach (var editHistory in allExistingEditHistory)
                {
                    EditHistoryInfo editHistoryInfo = new EditHistoryInfo();
                    editHistoryInfo.UserId = editHistory.UserId;
                    editHistoryInfo.UserName = editHistory.UserName;
                    editHistoryInfo.ItemName = editHistory.ItemName;
                    editHistoryInfo.ChangedAmount = editHistory.ChangedAmount;
                    editHistoryInfo.IsReducedAmount = editHistory.IsReducedAmount;
                    editHistoryInfo.TransactionDate = editHistory.TransactionDate;

                    editHistoryList.Add(editHistoryInfo);
                }

                return editHistoryList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting data from the editHistory collection");
                throw ex;
            }
        }

    }
       
}
