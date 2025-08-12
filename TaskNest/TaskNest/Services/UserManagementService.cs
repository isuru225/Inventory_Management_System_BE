using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using TaskNest.Custom.Exceptions;
using TaskNest.Frontend.Models;
using TaskNest.FrontendModels;
using TaskNest.IServices;
using TaskNest.Models;

namespace TaskNest.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ILogger<UserManagementService> _logger;
        private readonly IConfiguration _configuration;
        private ITokenService _tokenService;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<ApplicationRole> _roleManager;
        private IMongoDbService _mongoDbService;
        private IEmailService _emailService;

        public UserManagementService
            (
            ILogger<UserManagementService> logger,
            IConfiguration configuration,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IMongoDbService mongoDbService,
            IEmailService emailService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _mongoDbService = mongoDbService;
            _emailService = emailService;
        }

        public async Task<TokenResult> Login(UserLogin userLogin)
        {
            //validate the login object
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(userLogin);

            bool isValid = Validator.TryValidateObject(userLogin, validationContext, validationResults, true);

            if (!isValid)
            {
                throw new InvalidRequestedDataException("Requested data is not valid");
            }


            //check whether the user is still exist
            try
            {
                var userExists = await _userManager.FindByEmailAsync(userLogin.UserName);

                if (userExists != null)
                {
                    //verify the combination of email and password is correct.
                    var isPasswordValid = await _userManager.CheckPasswordAsync(userExists, userLogin.Password);

                    if (!isPasswordValid)
                    {

                        throw new InvalidCredentialsException(100,"Password is incorrect"); 
                 
                    }
                    else
                    {
                        var claims = new List<Claim>
                            {
                                new Claim(JwtRegisteredClaimNames.Sub, userExists.Id.ToString()), // Subject claim
                                new Claim("name", userExists.UserName),                           // Custom claim for name
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),// JWT ID claim
                                new Claim("userId", userExists.Id.ToString()),            // Custom claim for name identifier
                                new Claim("fullName", userExists.FullName.ToString())
                            };

                        try
                        {
                            var roles = await _userManager.GetRolesAsync(userExists);
                            foreach (var role in roles)
                            {
                                claims.Add(new Claim("role", role));
                            }

                            TokenResult tokenObject = _tokenService.CreateToken(claims);
                            return tokenObject;

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occured while getting user roles.");
                            throw ex;
                        }
                    }
                }
                else
                {
                    throw new UserNotFoundException(101,"Can not find an user by provided email"); 
      
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while finding user by email");
                throw ex;
            }


        }

        public async Task<String> CreateRole(CreateRole createRole)
        {
            var appRole = new ApplicationRole
            {
                Name = createRole.RoleName
            };

            try
            {
                var createRoleResult = await _roleManager.CreateAsync(appRole);
                return "Role added successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while creating user role.");
                throw ex;
            }
        }
        public async Task<RegisterResponse> RegisteredUser(UserRegisterInfo userRegisterInfo)
        {
            //validate the login object
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(userRegisterInfo);

            bool isValid = Validator.TryValidateObject(userRegisterInfo, validationContext, validationResults, true);

            if (!isValid)
            {
                throw new InvalidRequestedDataException("Incoming data is invalid");
            }

            //check whether the user is still exist
            try
            {
                var userExists = await _userManager.FindByEmailAsync(userRegisterInfo.Email);
                if (userExists != null)
                {
                    return new RegisterResponse
                    {
                        Message = "User is already exist",
                        Success = false
                    };
                }
                else
                {
                    //create new ApplicationUser
                    var user = new ApplicationUser
                    {
                        UserName = userRegisterInfo.Email,
                        Email = userRegisterInfo.Email,
                        FullName = userRegisterInfo.FirstName + " " + userRegisterInfo.LastName,
                        PhoneNumber = userRegisterInfo.MobileNumber
                    };

                    try
                    {
                        var result = await _userManager.CreateAsync(user, userRegisterInfo.Password);

                        if (result.Succeeded)
                        {
                            //add user roles

                            try
                            {
                                var addUserToRoleResult = await _userManager.AddToRoleAsync(user, userRegisterInfo.Role);
                                if (addUserToRoleResult.Succeeded)
                                {
                                    return new RegisterResponse
                                    {
                                        Message = "Registration Successful",
                                        Success = true
                                    };
                                }
                                else
                                {
                                    return new RegisterResponse
                                    {
                                        Message = "User created successfully. But adding roles failed",
                                        Success = false
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "An error occured while add roles");
                                throw ex;
                            }
                        }
                        else
                        {
                            return new RegisterResponse
                            {
                                Message = "Create user failed",
                                Success = false
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while registering a new user.");
                        throw ex;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while finding an user by email.");
                throw ex;
            }
        }

        public async Task<ApplicationUser> GetEmployeeInfo(string userName)
        {
            try
            {
                var employee = await _userManager.FindByEmailAsync(userName);

                if (employee == null)
                {
                    throw new UserNotFoundException(101, "Can not find an user by provided email");
                }

                return employee;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting employee info from Application User");
                throw ex;
            }
        }

        public async Task<List<RegisteredUsersInfo>> GetRegisteredUser() 
        {
            try
            {
                var filter = Builders<ApplicationUser>.Filter.Empty;
                var projection = Builders<ApplicationUser>.Projection.Expression(u => new ApplicationUser
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Roles = u.Roles
                });

                var registeredEmployees = await _mongoDbService.applicationUsers
                    .Find(filter)
                    .Project(projection)
                    .ToListAsync();


                try
                {
                    var roleList = await getRelevantUserRoleTypes();
                    List<RegisteredUsersInfo> userRegisterInfos = new List<RegisteredUsersInfo>();
                    foreach (var registeredEmployee in registeredEmployees)
                    {
                        RegisteredUsersInfo registeredUsersInfo = new RegisteredUsersInfo();
                        registeredUsersInfo.FullName = registeredEmployee.FullName;
                        registeredUsersInfo.Email = registeredEmployee.Email;
                        registeredUsersInfo.MobileNumber = registeredEmployee.PhoneNumber;
                        registeredUsersInfo.Id = registeredEmployee.Id;

                        List<String> roles = new List<String>();

                        foreach (var roleId in registeredEmployee.Roles) 
                        {
                            foreach (var role in roleList)
                            {
                                if (roleId == role.Id) 
                                {
                                    roles.Add(role.Name);
                                    break;
                                }
                            }

                        }

                        registeredUsersInfo.Roles = roles;
                        userRegisterInfos.Add(registeredUsersInfo);
                        
                    }

                    if (registeredEmployees.Count == 0)
                    {
                        throw new UserNotFoundException(404, "No registered users available");
                    }

                    return userRegisterInfos;
                }
                catch (Exception ex) 
                {
                    throw ex;
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public async Task<object> DeleteRegisteredUser(string userId)
        {
            try
            {

                var filter = Builders<ApplicationUser>.Filter.Eq(doc => doc.Id, userId);
                var result = await _mongoDbService.applicationUsers.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    return new
                    {
                        Message = "User deleted successfully.",
                        Success = true
                    };
                }
                else
                {
                    return new
                    {
                        Message = "No record found with the given Id.",
                        Success = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while deleting the history record in the DB.");
                throw ex;
            }
        }

        public async Task<List<ApplicationRole>> getRelevantUserRoleTypes() 
        {
            try 
            {
                var filter = Builders<ApplicationRole>.Filter.Empty;
                var result = await _mongoDbService.applicationRoles.Find(filter).ToListAsync();
                return result;
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        public async Task<object> forgotPassword(ForgetPasswordRequest forgetPasswordRequest) 
        {
            try
            {
                var employee = await _userManager.FindByEmailAsync(forgetPasswordRequest.Email);

                if (employee == null) 
                {
                    throw new UserNotFoundException(101, "Can not find an user by provided email");
                }

                try
                {
                    var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(employee);

                    // Encode token for URL
                    var encodedPasswordResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));

                    var resetUrl = $"{forgetPasswordRequest.ClientURI}/reset-password?email={Uri.EscapeDataString(forgetPasswordRequest.Email)}&token={encodedPasswordResetToken}";

                    var emailContent = $"Click the following link to reset your password of the Inventory Management System: <a href='{resetUrl}'>Reset Password</a>";

                    try
                    {
                        await _emailService.SendEmailAsync(forgetPasswordRequest.Email,"Reset Password", emailContent);

                        return new
                        {
                            message = "A password reset email has been sent",
                            isSuccessful = true
                        };
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex,"An error occured while sending the forgot password email.");
                        throw ex;
                    }
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex,"An error occured while generating the password reset token");
                    throw ex;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError("An error occured while finding the user by email.");
                throw ex;
            }
        }

        public async Task<object> resetPassword(ResetPasswordRequest resetPasswordRequest) 
        {
            try
            {
                var employee = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);

                if (employee == null)
                {
                    throw new UserNotFoundException(101, "Can not find an user by provided email");
                }

                try
                {
                    // Decode token from URL-safe Base64
                    var decodedPasswordResetTokenBytes = WebEncoders.Base64UrlDecode(resetPasswordRequest.PasswordResetToken);
                    var decodedPasswordResetToken = Encoding.UTF8.GetString(decodedPasswordResetTokenBytes);

                    var resetPassResult = await _userManager.ResetPasswordAsync(employee, decodedPasswordResetToken, resetPasswordRequest.NewPassword);

                    if (!resetPassResult.Succeeded)
                    {
                        var errors = resetPassResult.Errors.Select(e => e.Description);
                        return new
                        {
                            message = $"Error : {errors}",
                            isSuccessful = false
                        };
                    }

                    return new
                    {
                        message = "Password has been reset successfully.",
                        isSuccessful = true
                    };
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex,"An error occured while resetting the password.");
                    throw ex;
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occured while finding the user by email.");
                throw ex;
            }
           

        }
    }
}
