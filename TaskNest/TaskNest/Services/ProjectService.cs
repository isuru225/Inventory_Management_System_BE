using MongoDB.Bson;
using MongoDB.Driver;
using TaskNest.Frontend.Models;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class ProjectService : IProjectService
    {
        private IMongoDbService _mongoDbService;
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(IMongoDbService mongoDbService, ILogger<ProjectService> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
        }

        public async Task<object> CreateTask(TaskInfo taskInfo)
        {
            try
            {
                //Create new Projet Task
                ProjectTask projectTask = new ProjectTask();
                //generate new project Task Id
                projectTask.Id = ObjectId.GenerateNewId().ToString();
                projectTask.ProjectId = taskInfo.ProjectId;
                projectTask.TaskTitle = taskInfo.TaskTitle;
                projectTask.Description = taskInfo.Description;
                projectTask.DueDate = taskInfo.DueDate;
                projectTask.CreatedDate = taskInfo.CreatedDate;

                //get employee information
                try
                {
                    //var filter = Builders<ApplicationUser>.Filter.Eq(x => x.Id, taskInfo.CreatedEmployeeId);

                    //var result = await _mongoDbService.ApplicationUsers.Find(filter).FirstOrDefaultAsync();

                    //get all available employee infos
                    var filter = Builders<ApplicationUser>.Filter.Empty;  // Fetch all documents
                    var result = await _mongoDbService.applicationUsers.FindAsync(filter);  // Get async cursor

                    var allEmployeeInfos = await result.ToListAsync();

                    var taskCreatedEmployee = allEmployeeInfos.Find(x => x.Id == taskInfo.CreatedEmployeeId);

                    EmployeeInfo employeeInfo = new EmployeeInfo();
                    employeeInfo.EmployeeId = taskCreatedEmployee?.Id;
                    employeeInfo.EmployeeName = taskCreatedEmployee?.FullName;
                    employeeInfo.Email = taskCreatedEmployee?.Email;
                    employeeInfo.PhoneNumber = taskCreatedEmployee?.PhoneNumber;


                    //add employeeInfo

                    projectTask.CreatedEmployeeInfo = employeeInfo;
                    projectTask.Status = taskInfo.Status;
                    projectTask.PriorityLevel = taskInfo.PriorityLevel;
                    //Get assigned employee information from the db and insert it into the project task

                    List<EmployeeInfo> assignedEmployeesForTask = new List<EmployeeInfo>();
                    foreach (var assignedEmployeeId in taskInfo.AssignedEmployeeIds)
                    {
                        var assignedEmployeeInfo = allEmployeeInfos.Find(x => x.Id == assignedEmployeeId);
                        EmployeeInfo assignedEmployee = new EmployeeInfo();
                        assignedEmployee.EmployeeId = assignedEmployeeInfo.Id;
                        assignedEmployee.EmployeeName = assignedEmployeeInfo.FullName;
                        assignedEmployee.Email = assignedEmployeeInfo.Email;
                        assignedEmployee.PhoneNumber = assignedEmployeeInfo.PhoneNumber;

                        assignedEmployeesForTask.Add(assignedEmployee);
                    }

                    projectTask.AssignedEmployeeInfos = new List<EmployeeInfo>();
                    projectTask.AssignedEmployeeInfos = assignedEmployeesForTask;

                    try
                    {
                        await _mongoDbService.ProjectTasks.InsertOneAsync(projectTask);
                        return new
                        {
                            message = "Project task is successfully created",
                            isSuccessful = true
                        };

                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex,"An error occured insert values into ProjectTask Collection");
                        throw ex;
                    }

                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex,"An error occured while getting information from ApplicationUser table");
                    throw ex; 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating a task in the TaskInfo table");
                throw ex;
            }
        }

        public async Task<List<ProjectTask>> GetProjectSpecificTask(string projectId)
        {
            try
            {
                var filter = Builders<ProjectTask>.Filter.Eq(x => x.ProjectId, projectId);
                var result = await _mongoDbService.ProjectTasks.FindAsync<ProjectTask>(filter);

                //Create a list of Project tasks
                List<ProjectTask> projectTasks = new List<ProjectTask>();
                //add employee info into the object

                projectTasks = result.ToList();

                return projectTasks;

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting project tasks details from the project task table");
                throw ex;
            }
        }
    }
}
