using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection;
using TaskNest.Custom.Exceptions;
using TaskNest.Enum;
using TaskNest.Frontend.Models;
using TaskNest.Helper;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinishedDrugService : IFinishedDrugService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        private IHistoryService _historyService;
        private INotificationService _notificationService;

        public FinishedDrugService(IMongoDbService mongoDbService, ILogger<RawDrugService> logger, IHistoryService historyService, INotificationService notificationService)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _historyService = historyService;
            _notificationService = notificationService;
        }

        public async Task<Object> AddNewFinishedDrug(FinishedDrugInfo finishedDrugInfo)
        {
            var (isValid, errors) = ValidationHelper.ValidateObject(finishedDrugInfo);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            //check that the entered finish drug item is already exists in the db.
            FinishedDrug alreadyExistingfinishedDrug = null;

            try
            {
                var filter = Builders<FinishedDrug>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.FinishedDrugs.FindAsync(filter);  // Get async cursor
                var allExistingFinishedDrugs = await result.ToListAsync();
                alreadyExistingfinishedDrug = allExistingFinishedDrugs.Find(x => x.ItemName == finishedDrugInfo.ItemName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting the available finished drugs from the db.");
                throw;
            }

            if (alreadyExistingfinishedDrug != null)
            {
                throw new DuplicateValueException((int)ErrorCodes.DUPLICATE_VALUES, "Finished drug name already exists");
            }
            else
            {
                try
                {
                    //create new raw drug instance
                    FinishedDrug finishedDrug = new FinishedDrug();
                    finishedDrug.Amount = finishedDrugInfo.Amount;
                    finishedDrug.ItemName = finishedDrugInfo.ItemName;
                    finishedDrug.MeasurementUnit = finishedDrugInfo.MeasurementUnit;
                    finishedDrug.ExpirationDate = finishedDrugInfo.ExpirationDate;
                    finishedDrug.Category = finishedDrugInfo.Category;
                    finishedDrug.ReorderPoint = finishedDrugInfo.ReorderPoint;
                    finishedDrug.Id = ObjectId.GenerateNewId().ToString();

                    await _mongoDbService.FinishedDrugs.InsertOneAsync(finishedDrug);
                    return new
                    {
                        message = "New finished drug is successfully added",
                        isSuccessful = true
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while writting data into finished drug db collection");
                    throw;
                }
            }
        }

        public async Task<List<FinishedDrugInfo>> GetAllFinishedDrugs()
        {
            try
            {
                var filter = Builders<FinishedDrug>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.FinishedDrugs.FindAsync(filter);  // Get async cursor

                var allExistingFinishedDrugs = await result.ToListAsync();
                List<FinishedDrugInfo> finishedDrugList = new List<FinishedDrugInfo>();

                foreach (var finishedDrug in allExistingFinishedDrugs)
                {
                    FinishedDrugInfo finishedDrugInfo = new FinishedDrugInfo();
                    finishedDrugInfo.ItemName = finishedDrug.ItemName;
                    finishedDrugInfo.Category = finishedDrug.Category;
                    finishedDrugInfo.ExpirationDate = finishedDrug.ExpirationDate;
                    finishedDrugInfo.Amount = finishedDrug.Amount;
                    finishedDrugInfo.MeasurementUnit = finishedDrug.MeasurementUnit;
                    finishedDrugInfo.Id = finishedDrug.Id;
                    finishedDrugInfo.ReorderPoint = finishedDrug.ReorderPoint;

                    finishedDrugList.Add(finishedDrugInfo);
                }

                return finishedDrugList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting data from the finisheddrugs collection");
                throw;
            }
        }

        public async Task<FinishedDrug> GetFinishedDrugById(string Id)
        {
            try
            {
                var filter = Builders<FinishedDrug>.Filter.Eq(doc => doc.Id, Id);
                return await _mongoDbService.FinishedDrugs.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting finished drug document by using Id");
                throw;
            }
        }

        public async Task<Object> UpdateFinishedDrug(string Id, FinishedDrugInfo finishedDrugUpdatedValues)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(finishedDrugUpdatedValues);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }


            try
            {

                var filter = Builders<FinishedDrug>.Filter.Eq(d => d.Id, finishedDrugUpdatedValues.Id);
                var update = Builders<FinishedDrug>.Update
                    .Set(d => d.ItemName, finishedDrugUpdatedValues.ItemName)
                    .Set(d => d.ExpirationDate, finishedDrugUpdatedValues.ExpirationDate)
                    .Set(d => d.Category, finishedDrugUpdatedValues.Category)
                    .Set(d => d.Amount, finishedDrugUpdatedValues.Amount)
                    .Set(d => d.ReorderPoint, finishedDrugUpdatedValues.ReorderPoint)
                    .Set(d => d.MeasurementUnit, finishedDrugUpdatedValues.MeasurementUnit);

                var result = await _mongoDbService.FinishedDrugs.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return new
                    {
                        message = "Document is successfully updated",
                        finishedDrugId = Id,
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "No document matched the ID",
                        finishedDrugId = Id,
                        isSuccessful = false
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the full error details for debugging
                _logger.LogError(ex, "An error occurred while updating the given finished drug document");

                // Return a generic error response
                return new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    isSuccessful = false
                };
            }
        }

        public async Task<Object> UpdateFinishedDrugInventory(string Id, InventoryUpdate finishedDrugUpdatedValues)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(finishedDrugUpdatedValues);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }


            var finishedDrug = GetFinishedDrugById(Id);
            double? changedAmount = finishedDrug?.Result.Amount;

         
            var filter = Builders<FinishedDrug>.Filter.Eq(doc => doc.Id, Id);
            // Build the update definition dynamically
            var updateDefinitionBuilder = Builders<FinishedDrug>.Update;


            // Check if the property exists and matches the value
            PropertyInfo property = finishedDrugUpdatedValues?.GetType().GetProperty("Balance");
            if (property != null)
            {
                double balaceAmount = finishedDrugUpdatedValues.Balance;
                var update = updateDefinitionBuilder.Set(d => d.Amount, balaceAmount);
                
                // store the result of the updating process
                UpdateResult result;

                try
                {
                    // Update the raw Drugs Collection
                    result = await _mongoDbService.FinishedDrugs.UpdateOneAsync(filter, update);
                }
                catch (Exception ex)
                {
                    // Log the full error details for debugging
                    _logger.LogError(ex, "An error occurred while updating the given finished drug document");

                    // Return a generic error response
                    return new
                    {
                        message = "An unexpected error occurred. Please try again later.",
                        isSuccessful = false
                    };
                }

                if (result.ModifiedCount > 0)
                {
                    //Add history record
                    HistoryInfo historyInfo = new HistoryInfo();
                    historyInfo.AdjustedAmount = finishedDrugUpdatedValues.AmountAdjusted;
                    historyInfo.AdjustmentType = finishedDrugUpdatedValues.AdjustmentType;
                    historyInfo.CurrentAmount = finishedDrugUpdatedValues.Balance;
                    historyInfo.InitialAmount = finishedDrugUpdatedValues.InitialAmount;
                    historyInfo.ItemName = finishedDrugUpdatedValues.ItemName;
                    historyInfo.StoreKeeper = finishedDrugUpdatedValues.Author;
                    historyInfo.MeasurementUnit = finishedDrugUpdatedValues.MeasurementUnit;
                    historyInfo.Time = DateTime.Now;
                    historyInfo.Reason = finishedDrugUpdatedValues.Reason;

                    try
                    {
                        await _historyService.AddHistoryRecord(historyInfo);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    //Add a notification if current amount is less than reorder point.
                    if (finishedDrug?.Result.ReorderPoint > finishedDrugUpdatedValues?.Balance)
                    {
                        try
                        {
                            NotificationInfo notificationInfo = new NotificationInfo();
                            notificationInfo.CreatedAt = DateTime.UtcNow;
                            notificationInfo.NotificationType = (int)NotificationEnum.NOTIFICATION_TYPE_REORDER;
                            notificationInfo.ItemType = (int)ItemType.FINISHED_DRUG;
                            notificationInfo.ItemName = finishedDrugUpdatedValues.ItemName;

                            await _notificationService.AddNotification(notificationInfo);
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }

                    return new
                    {
                        message = "Document is successfully updated",
                        finishedDrugId = Id,
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "Document is not successfully updated",
                        finishedDrugId = Id,
                        isSuccessful = false
                    };
                }
            }
            else
            {
                //throw new AttributeNotFoundException("Expected attribute does not exist");
                return new
                {
                    message = "Balance is missing in the provided request",
                    isSuccessful = false
                };
            }

        }
        public async Task<object> DeleteFinishedDrug(string finishedDrugId)
        {
            try
            {

                var filter = Builders<FinishedDrug>.Filter.Eq(doc => doc.Id, finishedDrugId);
                var result = await _mongoDbService.FinishedDrugs.DeleteOneAsync(filter);

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
                _logger.LogError(ex, "An error occured while deleting the finished drug record in the DB.");
                throw;
            }
        }
    }
}
