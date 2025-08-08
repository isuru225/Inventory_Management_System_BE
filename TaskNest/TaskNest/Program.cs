
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository;
using MongoDB.Driver;
using TaskNest.Models;
using TaskNest.FrontendModels;
using Microsoft.AspNetCore.Mvc;
using TaskNest.IServices;
using TaskNest.Services;
using TaskNest.Frontend.Models;
using TaskNest.Config.Models;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data;

namespace TaskNest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // MongoDB Connection String
            var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb");

            // Add MongoDB Identity configuration
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddMongoDbStores<ApplicationUser, ApplicationRole, string>(
                    mongoConnectionString, "InventoryManagementDb") // MongoDB connection and database name
                .AddDefaultTokenProviders()
                .AddUserManager<UserManager<ApplicationUser>>()
                .AddSignInManager<SignInManager<ApplicationUser>>()
                .AddRoleManager<RoleManager<ApplicationRole>>();


            // Configure Identity options if needed (e.g., password rules)
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            });

            // Register MongoDB client if you need it elsewhere in the app
            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var client = new MongoClient(mongoConnectionString);
                return client;
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //JWT Settings
            //JWT

            var jwtSettings = new JWTSettings();
            builder.Configuration.Bind("Jwt", jwtSettings);

            // Configure JWTSettings for DI
            var jwtSection = builder.Configuration.GetSection("Jwt");
            builder.Services.Configure<JWTSettings>(jwtSection);

            //////////////////
            builder.Services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.SaveToken = true;
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Key ??
                   throw new InvalidOperationException())),
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidIssuer = jwtSettings.Issuer,
                   ValidAudience = jwtSettings.Audience,
                   RequireExpirationTime = false
               };
               options.Audience = jwtSettings.Audience;
               options.ClaimsIssuer = jwtSettings.Issuer;
               options.Events = new JwtBearerEvents
               {
                   OnAuthenticationFailed = context =>
                   {
                       if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                       {
                           context.Response.Headers.Add("Token-Expired", "true");
                       }
                       return Task.CompletedTask;
                   }
               };
           });

            //register the services

            builder.Services.AddScoped<IUserManagementService, UserManagementService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton<IMongoDbService, MongoDbService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IRawDrugService, RawDrugService>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFinishedDrugService, FinishedDrugService>();
            builder.Services.AddScoped<IGeneralStoreService, GeneralStoreService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins("http://localhost:3000") // Adjust origin as needed
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials(); // Allow credentials (cookies)
                });
            });


            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            // Use the CORS policy
            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();


            //user management minimal APIs

            app.MapPost("/login", async ([FromBody] UserLogin userLogin, IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.Login(userLogin);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }

            });

            app.MapPost("/register", async ([FromBody] UserRegisterInfo userRegisterInfo, IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.RegisteredUser(userRegisterInfo);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapPost("/addrole", async ([FromBody] CreateRole createRole, IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.CreateRole(createRole);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapGet("dashboard/getuser", async ([FromQuery] string email, IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.GetEmployeeInfo(email);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //Home minimal AIPs

            app.MapGet("/home/getprojects", async (IHomeService homeService) =>
            {
                try
                {
                    var result = await homeService.GetAllProjects();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapPost("/admin/addproject", async ([FromBody] ProjectInfo projectInfo, IAdminService adminService) =>
            {
                try
                {
                    var result = await adminService.AddProjects(projectInfo);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //Project Minimal APIs

            app.MapPost("/project/createprojecttask", async ([FromBody] TaskInfo taskInfo, IProjectService projectService) =>
            {
                try
                {
                    var result = await projectService.CreateTask(taskInfo);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapGet("/project/gettasks", async ([FromQuery] string projectId, IProjectService projectService) =>
            {

                try
                {
                    var result = await projectService.GetProjectSpecificTask(projectId);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //Raw drug minimal APIs
            app.MapPost("/addrawdrug", [Authorize(Roles = "Admin")] async ([FromBody] RawDrugInfo rawDrugInfo, IRawDrugService rawDrugService) =>
            {

                try
                {
                    var result = await rawDrugService.AddNewRawDrug(rawDrugInfo);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapGet("/getrawdrugs", [Authorize] async (IRawDrugService rawDrugService) =>
            {
                try
                {
                    var result = await rawDrugService.GetAllRawDrugs();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapPut("/updaterawdrug/{id}", [Authorize(Roles = "Admin")] async (string Id, RawDrugInfo updateRawDrugValues, IRawDrugService rawDrugService) =>
            {
                try
                {
                    var result = await rawDrugService.UpdateRawDrug(Id, updateRawDrugValues);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapDelete("/deleterawdrug/{id}", [Authorize(Roles = "Admin")] async (string Id, IRawDrugService rawDrugService) =>
            {
                try
                {
                    var result = await rawDrugService.DeleteRawDrug(Id);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //update the rawdrug inventory through storekeeper
            app.MapPut("/updaterawdruginventory/{id}", [Authorize(Roles = "Admin")] async (string Id, InventoryUpdate updateValues, IRawDrugService rawDrugService) =>
            {
                try
                {
                    var result = await rawDrugService.UpdateRawDrugInventory(Id,updateValues);
                    return Results.Ok(result);

                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //update the finisheddrug inventory through storekeeper
            app.MapPut("/updatefinisheddruginventory/{id}", [Authorize(Roles = "Admin")] async (string Id, InventoryUpdate updateValues, IFinishedDrugService finishedDrugService) =>
            {
                try
                {
                    var result = await finishedDrugService.UpdateFinishedDrugInventory(Id, updateValues);
                    return Results.Ok(result);

                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });


            //get all the history records
            app.MapGet("/gethistoryrecords", [Authorize(Roles = "Admin")] async (IHistoryService historyService) =>
            {
                try
                {
                    var result = await historyService.GetAllHistoryRecord();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //delete a selected history record
            app.MapDelete("/deletehistoryrecord/{id}", [Authorize(Roles = "Admin")] async (string Id, IHistoryService historyService) =>
            {
                try
                {
                    var result = await historyService.DeleteHistoryRecord(Id);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });
            ////////////////////////



            //Raw drug minimal APIs
            app.MapPost("/addfinisheddrug", [Authorize(Roles = "Admin")] async ([FromBody] FinishedDrugInfo finishedDrugInfo, IFinishedDrugService finishedDrugService) =>
            {

                try
                {
                    var result = await finishedDrugService.AddNewFinishedDrug(finishedDrugInfo);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapGet("/getfinisheddrugs", [Authorize] async (IFinishedDrugService finishedDrugService) =>
            {
                try
                {
                    var result = await finishedDrugService.GetAllFinishedDrugs();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapPut("/updatefinisheddrug/{id}", [Authorize(Roles = "Admin")] async (string Id, FinishedDrugInfo updateFinishedDrugValues, IFinishedDrugService finishedDrugService) =>
            {
                try
                {
                    var result = await finishedDrugService.UpdateFinishedDrug(Id, updateFinishedDrugValues);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapDelete("/deletefinisheddrug/{id}", [Authorize(Roles = "Admin")] async (string Id, IFinishedDrugService finishedDrugService) =>
            {
                try
                {
                    var result = await finishedDrugService.DeleteFinishedDrug(Id);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            /////////////////////////////
            app.MapPost("/addgeneralstoreitem", [Authorize(Roles = "Admin")] async ([FromBody] GeneralStoreItemInfo generalStoreItemInfo, IGeneralStoreService generalStoreService) =>
            {

                try
                {
                    var result = await generalStoreService.AddNewGeneralStoreItem(generalStoreItemInfo);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapGet("/getgeneralstoreitems", [Authorize] async (IGeneralStoreService generalStoreService) =>
            {
                try
                {
                    var result = await generalStoreService.GetAllGeneralStoreItems();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapPut("/updategeneralstoreitem/{id}", [Authorize(Roles = "Admin")] async (string Id, GeneralStoreItemInfo updateGeneralStoreItemValues, IGeneralStoreService generalStoreService) =>
            {
                try
                {
                    var result = await generalStoreService.UpdateGeneralStoreItem(Id, updateGeneralStoreItemValues);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            app.MapDelete("/deletegeneralstoreitem/{id}", [Authorize(Roles = "Admin")] async (string Id, IGeneralStoreService generalStoreService) =>
            {
                try
                {
                    var result = await generalStoreService.DeleteGeneralStoreItemDrug(Id);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });

            //get all the notifications
            app.MapGet("/getnotifications", [Authorize]  async (INotificationService notificationService) =>
            {
                try
                {
                    var result = await notificationService.GetAllNotification();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });
            //get all registered users
            app.MapGet("/getregisteredusers", [Authorize] async (IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.GetRegisteredUser();
                    return Results.Ok(result);
                }
                catch (Exception ex) 
                {
                    return Results.BadRequest(ex);
                }
            });
            //delete a selected registered user record
            app.MapDelete("/deleteregistereduser/{id}", [Authorize(Roles = "Admin")] async (string Id, IUserManagementService userManagementService) =>
            {
                try
                {
                    var result = await userManagementService.DeleteRegisteredUser(Id);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex);
                }
            });


            app.Run();
        }
    }
}
