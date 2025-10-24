using TaskNest.IServices;
using MongoDB.Driver;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class HomeService : IHomeService
    {
        private IMongoDbService _mongoDbService;
        private readonly ILogger _logger;
        public HomeService(IMongoDbService mongoDbService,ILogger<HomeService> logger) 
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<List<Project>> GetAllProjects()
        {
            try 
            {
                var result = await _mongoDbService.Projects.Find(_ => true).ToListAsync();
                return result;

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting all the available projects.");
                throw;
            }
        }
    }
}
