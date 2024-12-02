using MongoDB.Bson;
using MongoDB.Driver;
using TaskNest.Frontend.Models;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class AdminService : IAdminService
    {
        private IMongoDbService _mongoDbService;
        private readonly ILogger<AdminService> _logger;
        public AdminService(IMongoDbService mongoDbService,ILogger<AdminService> logger) 
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<Object> AddProjects(ProjectInfo projectInfo) 
        {
            try
            {
                //Create new project
                Project project = new Project();
                //generate new project Id
                project.Id = ObjectId.GenerateNewId().ToString();
                project.ProjectName = projectInfo.ProjectName;
                project.Description = projectInfo.Description;
                project.ApplicationUsers = new List<string>();
                //add application user names
                project.ApplicationUsers = projectInfo.EmployeeIds.ToList();
                //initialize ProjectTasks instance
                project.ProjectTasks = new List<ProjectTask>();
                //adding the available technologies into the db
                project.Technologies = new List<string>();
                project.Technologies = projectInfo.Technologies;
                //adding the project into the MongoDb 
                await _mongoDbService.Projects.InsertOneAsync(project);

                return new
                {
                    Message = "New project was succesfully added.",
                    isSuccessful = true
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occued while inserting data into the Projects collection");
                throw ex;
            }
        }
    }
}
