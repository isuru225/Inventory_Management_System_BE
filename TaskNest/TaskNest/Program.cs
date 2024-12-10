
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
            builder.Services.AddScoped<ITokenService,TokenService>();
            builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("Jwt"));
            builder.Services.AddSingleton<IMongoDbService, MongoDbService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IHomeService, HomeService>();
            builder.Services.AddScoped<IProjectService,ProjectService>();
            builder.Services.AddScoped<IRawDrugService, RawDrugService>();

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
                    var result =  await userManagementService.RegisteredUser(userRegisterInfo); 
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
            app.MapPost("/addrawdrug", [Authorize(Roles ="Admin")] async ([FromBody] RawDrugInfo rawDrugInfo, IRawDrugService rawDrugService) =>
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

            app.MapPut("/updaterawdrug/{id}", [Authorize(Roles = "Admin")] async (string Id, Dictionary<string, object> updateValues, IRawDrugService rawDrugService) =>
            {
                try
                {
                    var result = await rawDrugService.UpdateRawDrug(Id,updateValues);
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

            app.Run();
        }
    }
}
