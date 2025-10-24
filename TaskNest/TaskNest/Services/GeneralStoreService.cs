using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public class GeneralStoreService : IGeneralStoreService
    {
        private readonly IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        private IHistoryService _historyService;
        public GeneralStoreService(IMongoDbService mongoDbService, ILogger<RawDrugService> logger, IHistoryService historyService)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _historyService = historyService;
        }

        public async Task<Object> AddNewGeneralStoreItem(GeneralStoreItemInfo generalStoreItemInfo)
        {
            var (isValid, errors) = ValidationHelper.ValidateObject(generalStoreItemInfo);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            GeneralStoreItem alreadyExistingRawDrug = null;

            try
            {
                var filter = Builders<GeneralStoreItem>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.GeneralStoreItems.FindAsync(filter);  // Get async cursor
                var allExistingGeneralStoreItems = await result.ToListAsync();

                alreadyExistingRawDrug = allExistingGeneralStoreItems.Find(x => x.ItemName == generalStoreItemInfo.ItemName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting drug infos from the general store item collection");
                throw;
            }

            if (alreadyExistingRawDrug != null)
            {
                throw new DuplicateValueException((int)ErrorCodes.DUPLICATE_VALUES, "General store item is already exists");
            }
            else
            {
                try
                {
                    //create new raw drug instance
                    GeneralStoreItem generalStoreItem = new GeneralStoreItem();
                    generalStoreItem.Amount = generalStoreItemInfo.Amount;
                    generalStoreItem.ItemName = generalStoreItemInfo.ItemName;
                    generalStoreItem.Id = ObjectId.GenerateNewId().ToString();

                    await _mongoDbService.GeneralStoreItems.InsertOneAsync(generalStoreItem);
                    return new
                    {
                        message = "New general store item is successfully added",
                        isSuccessful = true
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while writting data into general store item collection");
                    throw;
                }
            }
        }

        public async Task<List<GeneralStoreItemInfo>> GetAllGeneralStoreItems()
        {
            try
            {
                var filter = Builders<GeneralStoreItem>.Filter.Empty;  // Fetch all documents
                var result = await _mongoDbService.GeneralStoreItems.FindAsync(filter);  // Get async cursor

                var allExistingGeneralStoreItems = await result.ToListAsync();
                List<GeneralStoreItemInfo> generalStoreItemInfoList = new List<GeneralStoreItemInfo>();

                foreach (var generalStoreItem in allExistingGeneralStoreItems)
                {
                    GeneralStoreItemInfo generalStoreItemInfo = new GeneralStoreItemInfo();
                    generalStoreItemInfo.ItemName = generalStoreItem.ItemName;
                    generalStoreItemInfo.Amount = generalStoreItem.Amount;
                    generalStoreItemInfo.Id = generalStoreItem.Id;

                    generalStoreItemInfoList.Add(generalStoreItemInfo);
                }

                return generalStoreItemInfoList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting data from the general store item collection");
                throw;
            }
        }

        public async Task<GeneralStoreItem> GetGeneralStoreItemById(string Id)
        {
            try
            {
                var filter = Builders<GeneralStoreItem>.Filter.Eq(doc => doc.Id, Id);
                return await _mongoDbService.GeneralStoreItems.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while getting general store item document by using Id");
                throw;
            }
        }

        public async Task<Object> UpdateGeneralStoreItem(string Id, GeneralStoreItemInfo generalStoreItemUpdatedValues)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(generalStoreItemUpdatedValues);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            try
            {

                var filter = Builders<GeneralStoreItem>.Filter.Eq(d => d.Id, generalStoreItemUpdatedValues.Id);
                var update = Builders<GeneralStoreItem>.Update
                    .Set(d => d.ItemName, generalStoreItemUpdatedValues.ItemName)
                    .Set(d => d.Amount, generalStoreItemUpdatedValues.Amount);

                var result = await _mongoDbService.GeneralStoreItems.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return new
                    {
                        message = "Document is successfully updated",
                        generalStoreItemId = Id,
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "No document matched the ID",
                        generalStoreItemId = Id,
                        isSuccessful = false
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the full error details for debugging
                _logger.LogError(ex, "An error occurred while updating the given general store item document");

                // Return a generic error response
                return new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    isSuccessful = false
                };
            }
        }

        public async Task<Object> UpdateGeneralStoreInventory(string Id, InventoryUpdate generalStoreItemUpdatedValues)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(generalStoreItemUpdatedValues);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            var generalStoreItem = GetGeneralStoreItemById(Id);
            double? changedAmount = generalStoreItem?.Result.Amount;


            var filter = Builders<GeneralStoreItem>.Filter.Eq(doc => doc.Id, Id);
            // Build the update definition dynamically
            var updateDefinitionBuilder = Builders<GeneralStoreItem>.Update;


            // Check if the property exists and matches the value
            PropertyInfo property = generalStoreItemUpdatedValues?.GetType().GetProperty("Balance");
            if (property != null)
            {
                double balaceAmount = generalStoreItemUpdatedValues.Balance;
                var update = updateDefinitionBuilder.Set(d => d.Amount, balaceAmount);

                UpdateResult result;
                try
                {
                    //// Update the raw Drugs Collection
                    result = await _mongoDbService.GeneralStoreItems.UpdateOneAsync(filter, update);
                }
                catch (Exception ex)
                {
                    // Log the full error details for debugging
                    _logger.LogError(ex, "An error occurred while updating the given general store item document");

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
                    historyInfo.AdjustedAmount = generalStoreItemUpdatedValues.AmountAdjusted;
                    historyInfo.AdjustmentType = generalStoreItemUpdatedValues.AdjustmentType;
                    historyInfo.CurrentAmount = generalStoreItemUpdatedValues.Balance;
                    historyInfo.InitialAmount = generalStoreItemUpdatedValues.InitialAmount;
                    historyInfo.ItemName = generalStoreItemUpdatedValues.ItemName;
                    historyInfo.StoreKeeper = generalStoreItemUpdatedValues.Author;
                    historyInfo.MeasurementUnit = generalStoreItemUpdatedValues.MeasurementUnit;
                    historyInfo.Time = DateTime.UtcNow;
                    historyInfo.Reason = generalStoreItemUpdatedValues.Reason;

                    try
                    {
                        await _historyService.AddHistoryRecord(historyInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while adding a history record from general store inventory updating process");
                        throw;
                    }

                    return new
                    {
                        message = "Document is successfully updated",
                        generalStoreId = Id,
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "Document is not successfully updated",
                        generalStoreId = Id,
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
        public async Task<object> DeleteGeneralStoreItemDrug(string generalStoreItemId)
        {
            try
            {

                var filter = Builders<GeneralStoreItem>.Filter.Eq(doc => doc.Id, generalStoreItemId);
                var result = await _mongoDbService.GeneralStoreItems.DeleteOneAsync(filter);

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
                _logger.LogError(ex, "An error occured while deleting the general store item record in the DB.");
                throw;
            }
        }
    }
}
