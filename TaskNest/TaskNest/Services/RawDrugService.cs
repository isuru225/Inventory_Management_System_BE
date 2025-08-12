using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using TaskNest.Custom.Exceptions;
using TaskNest.Enum;
using TaskNest.Frontend.Models;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawDrugService : IRawDrugService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        private IHistoryService _historyService;
        private INotificationService _notificationService;
        public RawDrugService(IMongoDbService mongoDbService, ILogger<RawDrugService> logger, IHistoryService historyService,INotificationService notificationService) 
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _historyService = historyService;
            _notificationService = notificationService;
        }

        public async Task<Object> AddNewRawDrug(RawDrugInfo rawDrugInfo)
        {
            try
            {

                var filter = Builders<RawDrug>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.RawDrugs.FindAsync(filter);  // Get async cursor

                var allExistingRawDrugs = await result.ToListAsync();

                var duplicateValue = allExistingRawDrugs.Find(x => x.ItemName == rawDrugInfo.ItemName);

                if (duplicateValue != null)
                {
                    throw new DuplicateValueException("Raw drug name already exists");
                }
                else 
                {
                    try
                    {
                        //create new raw drug instance
                        RawDrug rawDrug = new RawDrug();
                        rawDrug.Amount = rawDrugInfo.Amount;
                        rawDrug.ItemName = rawDrugInfo.ItemName;
                        rawDrug.MeasurementUnit = rawDrugInfo.MeasurementUnit;
                        rawDrug.ExpirationDate = rawDrugInfo.ExpirationDate;
                        rawDrug.Category = rawDrugInfo.Category;
                        rawDrug.ReorderPoint = rawDrugInfo.ReorderPoint;
                        rawDrug.Id = ObjectId.GenerateNewId().ToString();

                        await _mongoDbService.RawDrugs.InsertOneAsync(rawDrug);
                        return new
                        {
                            message = "New raw drug is successfully added",
                            isSuccessful = true
                        };
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex,"An error occured while writting data into raw drug collection");
                        throw ex;
                    }
                }

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting drug infos from the raw drug collection");
                throw ex;
            }
        }

        public async Task<List<RawDrugInfo>> GetAllRawDrugs() 
        {
            try
            {
                var filter = Builders<RawDrug>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.RawDrugs.FindAsync(filter);  // Get async cursor

                var allExistingRawDrugs = await result.ToListAsync();
                List<RawDrugInfo> rawDrugList = new List<RawDrugInfo>();

                foreach (var rawDrug in allExistingRawDrugs) 
                {
                    RawDrugInfo rawDrugInfo = new RawDrugInfo();
                    rawDrugInfo.ItemName = rawDrug.ItemName;
                    rawDrugInfo.Category = rawDrug.Category;
                    rawDrugInfo.ExpirationDate = rawDrug.ExpirationDate;
                    rawDrugInfo.Amount = rawDrug.Amount;
                    rawDrugInfo.MeasurementUnit = rawDrug.MeasurementUnit;
                    rawDrugInfo.Id = rawDrug.Id;
                    rawDrugInfo.ReorderPoint = rawDrug.ReorderPoint;
                    
                    rawDrugList.Add(rawDrugInfo);
                }

                return rawDrugList;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting data from the rawdrugs collection");
                throw ex;
            }
        }

        public async Task<RawDrug> GetRawDrugById(string Id) 
        {
            try
            {
                var filter = Builders<RawDrug>.Filter.Eq(doc => doc.Id, Id);
                return await _mongoDbService.RawDrugs.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while getting raw drug document by using Id");
                throw ex;
            }
        }

        public async Task<Object> UpdateRawDrug(string Id, RawDrugInfo rawDrugUpdatedValues)
        {

            //var rawDrug = GetRawDrugById(Id);
            //double? changedAmount = rawDrug?.Result.Amount;

            try
            {

                var filter = Builders<RawDrug>.Filter.Eq(d => d.Id, rawDrugUpdatedValues.Id);
                var update = Builders<RawDrug>.Update
                    .Set(d => d.ItemName, rawDrugUpdatedValues.ItemName)
                    .Set(d => d.ExpirationDate, rawDrugUpdatedValues.ExpirationDate)
                    .Set(d => d.Category, rawDrugUpdatedValues.Category)
                    .Set(d => d.Amount, rawDrugUpdatedValues.Amount)
                    .Set(d => d.ReorderPoint, rawDrugUpdatedValues.ReorderPoint)
                    .Set(d => d.MeasurementUnit, rawDrugUpdatedValues.MeasurementUnit);

                var result = await _mongoDbService.RawDrugs.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return new
                    {
                        message = "Document is successfully updated",
                        rawDrugId = Id,
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "No document matched the ID",
                        rawDrugId = Id,
                        isSuccessful = false
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the full error details for debugging
                _logger.LogError(ex, "An error occurred while updating the given raw drug document");

                // Return a generic error response
                return new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    isSuccessful = false
                };
            }
        }

        public async Task<Object> UpdateRawDrugInventory(string Id, InventoryUpdate rawDrugUpdatedValues)
        {

            var rawDrug = GetRawDrugById(Id);
            double? changedAmount = rawDrug?.Result.Amount;

            try
            {
                var filter = Builders<RawDrug>.Filter.Eq(doc => doc.Id, Id);
                // Build the update definition dynamically
                var updateDefinitionBuilder = Builders<RawDrug>.Update;


                // Check if the property exists and matches the value
                PropertyInfo property = rawDrugUpdatedValues?.GetType().GetProperty("Balance");
                if (property != null)
                {
                    double balaceAmount = rawDrugUpdatedValues.Balance;
                    var update = updateDefinitionBuilder.Set(d => d.Amount, balaceAmount);
                    // Update the raw Drugs Collection
                    var result = await _mongoDbService.RawDrugs.UpdateOneAsync(filter, update);

                    if (result.ModifiedCount > 0)
                    {
                        //Add history record
                        HistoryInfo historyInfo = new HistoryInfo();
                        historyInfo.AdjustedAmount = rawDrugUpdatedValues.AmountAdjusted;
                        historyInfo.AdjustmentType = rawDrugUpdatedValues.AdjustmentType;
                        historyInfo.CurrentAmount = rawDrugUpdatedValues.Balance;
                        historyInfo.InitialAmount = rawDrugUpdatedValues.InitialAmount;
                        historyInfo.ItemName = rawDrugUpdatedValues.ItemName;
                        historyInfo.StoreKeeper = rawDrugUpdatedValues.Author;
                        historyInfo.MeasurementUnit = rawDrugUpdatedValues.MeasurementUnit;
                        historyInfo.Time = DateTime.UtcNow;
                        historyInfo.Reason = rawDrugUpdatedValues.Reason;

                        //add the inventory transaction into the history
                        try
                        {
                            _historyService.AddHistoryRecord(historyInfo);
                        }
                        catch (Exception ex) 
                        {
                            throw ex;
                        }

                        //Add a notification if current amount is less than reorder point.
                        if (rawDrug?.Result.ReorderPoint>rawDrugUpdatedValues?.Balance) 
                        {
                            try
                            {
                                NotificationInfo notificationInfo = new NotificationInfo();
                                notificationInfo.CreatedAt = DateTime.Now;
                                notificationInfo.NotificationType = (int)NotificationEnum.NOTIFICATION_TYPE_REORDER;
                                notificationInfo.ItemType = (int)ItemType.RAW_DRUG;
                                notificationInfo.ItemName = rawDrugUpdatedValues.ItemName;

                                await _notificationService.AddNotification(notificationInfo);
                            }
                            catch (Exception ex) 
                            {
                                throw ex;
                            }
                        }

                        return new
                        {
                            message = "Document is successfully updated",
                            rawDrugId = Id,
                            isSuccessful = true,
                        };
                    }
                    else
                    {
                        return new
                        {
                            message = "Document is not successfully updated",
                            rawDrugId = Id,
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
            catch (Exception ex)
            {
                // Log the full error details for debugging
                _logger.LogError(ex, "An error occurred while updating the given raw drug document");

                // Return a generic error response
                return new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    isSuccessful = false
                };
            }
        }
        public async Task<object> DeleteRawDrug (string rawDrugId) 
        {
            try
            {

                var filter = Builders<RawDrug>.Filter.Eq(doc => doc.Id, rawDrugId);
                var result = await _mongoDbService.RawDrugs.DeleteOneAsync(filter);

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
                _logger.LogError(ex,"An error occured while deleting the raw drug record in the DB.");
                throw ex;
            }
        }


    }
}
