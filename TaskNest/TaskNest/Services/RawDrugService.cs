using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System.Text.Json;
using TaskNest.Custom.Exceptions;
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
        public RawDrugService(IMongoDbService mongoDbService, ILogger<RawDrugService> logger) 
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
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

        public async Task<Object> UpdateRawDrug(string Id, Dictionary<string, object> rawDrugUpdatedValues)
        {
            try
            {
                var filter = Builders<RawDrug>.Filter.Eq(doc => doc.Id, Id);
                // Build the update definition dynamically
                var updateDefinitionBuilder = Builders<RawDrug>.Update;
                var updateDefinition = new List<UpdateDefinition<RawDrug>>();

                foreach (var entry in rawDrugUpdatedValues)
                {
                    // Check if the value is a JsonElement
                    if (entry.Value is JsonElement jsonElement)
                    {
                        object convertedValue;
                        // Check the value type and convert accordingly
                        if (jsonElement.ValueKind == JsonValueKind.Number)
                        {
                            convertedValue = jsonElement.TryGetInt32(out int intValue) ? intValue : jsonElement.GetDouble();
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            convertedValue = jsonElement.GetString();
                        }
                        else
                        {
                            convertedValue = JsonSerializer.Deserialize<object>(jsonElement.GetRawText());
                        }
                        if (convertedValue != null)
                        {
                            updateDefinition.Add(updateDefinitionBuilder.Set(entry.Key, convertedValue));
                        }
                    }
                    else if (entry.Value != null) // Handle non-JsonElement values
                    {
                        updateDefinition.Add(updateDefinitionBuilder.Set(entry.Key, entry.Value));
                    }
                }

                // Combine all updates into a single update definition
                var combinedUpdate = updateDefinitionBuilder.Combine(updateDefinition);

                // Update the raw Drugs Collection
                var result = await _mongoDbService.RawDrugs.UpdateOneAsync(filter, combinedUpdate);

                if (result.ModifiedCount > 0)
                {
                    return new
                    {
                        message = "Document is successfully updated",
                        isSuccessful = true,
                    };
                }
                else
                {
                    return new
                    {
                        message = "No document matched the ID",
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


    }
}
