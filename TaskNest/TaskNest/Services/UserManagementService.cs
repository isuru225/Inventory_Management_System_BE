using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using TaskNest.Custom.Exceptions;
using TaskNest.Enum;
using TaskNest.Frontend.Models;
using TaskNest.FrontendModels;
using TaskNest.Helper;
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

            var (isValid, errors) = ValidationHelper.ValidateObject(userLogin);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            ApplicationUser? userExists;

            try
            {
                userExists = await _userManager.FindByEmailAsync(userLogin.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while finding the user by {userLogin?.UserName}");
                throw;
            }

            if (userExists != null)
            {
                Boolean isPasswordValid = false;
                try
                {
                    //verify the combination of email and password is correct.
                    isPasswordValid = await _userManager.CheckPasswordAsync(userExists, userLogin.Password);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while checking the provided password!");
                    throw;
                }

                if (!isPasswordValid)
                {
                    throw new InvalidCredentialsException((int)ErrorCodes.INVALID_PASSWORD, "Password is incorrect");
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
                    var roles = await _userManager.GetRolesAsync(userExists);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim("role", role));
                    }

                    TokenResult tokenObject = _tokenService.CreateToken(claims);
                    return tokenObject;

                }
            }
            else
            {
                throw new UserNotFoundException((int)ErrorCodes.INVALID_EMAIL, "Can not find an user by provided email");
            }

        }

        public async Task<String> CreateRole(CreateRole createRole)
        {
            var (isValid, errors) = ValidationHelper.ValidateObject(createRole);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

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
                throw;
            }
        }
        public async Task<RegisterResponse> RegisteredUser(UserRegisterInfo userRegisterInfo)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(userRegisterInfo);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            ApplicationUser? userExists;

            try
            {
                userExists = await _userManager.FindByEmailAsync(userRegisterInfo.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while finding the user by {userRegisterInfo?.Email}");
                throw;
            }

            if (userExists != null)
            {
                return new RegisterResponse
                {
                    Message = "User is already exist",
                    Success = false
                };
            }

            //create new ApplicationUser
            ApplicationUser user = new ApplicationUser
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
                _logger.LogError(ex, $"An error occured while registering new user using {userRegisterInfo.Email}");
                throw;
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
                _logger.LogError(ex, "An error occured while getting employee info from Application User");
                throw;
            }
        }

        public async Task<List<RegisteredUsersInfo>> GetRegisteredUser()
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

            List<ApplicationUser> registeredEmployees = new List<ApplicationUser>(); 

            try
            {
                registeredEmployees = await _mongoDbService.applicationUsers
                .Find(filter)
                .Project(projection)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"An error occured while getting registered users!");
                throw;
            }

           
            var roleList = await getRelevantUserRoleTypes();
            List<RegisteredUsersInfo> userRegisterInfos = new List<RegisteredUsersInfo>();
            foreach (ApplicationUser registeredEmployee in registeredEmployees)
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
                throw;
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
                _logger.LogError(ex, "An error occured while getting user role types.");
                throw;
            }
        }

        public async Task<object> forgotPassword(ForgetPasswordRequest forgetPasswordRequest)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(forgetPasswordRequest);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            //define application user
            ApplicationUser employee;

            try
            {
                employee = await _userManager.FindByEmailAsync(forgetPasswordRequest.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while finding the user by {forgetPasswordRequest?.Email}");
                throw;
            }

            if (employee == null)
            {
                throw new UserNotFoundException((int)ErrorCodes.INVALID_EMAIL, "Can not find an user by provided email");
            }


            string emailContent = "";

            try
            {
                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(employee);

                // Encode token for URL
                var encodedPasswordResetToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordResetToken));

                var resetUrl = $"{forgetPasswordRequest.ClientURI}/resetpassword?email={Uri.EscapeDataString(forgetPasswordRequest.Email)}&token={encodedPasswordResetToken}";

                emailContent = $"Click the following link to reset your password of the Inventory Management System: <a href='{resetUrl}'>Reset Password</a>";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while generating the password reset token in {forgetPasswordRequest.Email}");
                throw;
            }

            try
            {
                await _emailService.SendEmailAsync(forgetPasswordRequest.Email, "Reset Password", emailContent);

                return new
                {
                    message = "A password reset email has been sent",
                    isSuccessful = true
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while sending the password reset email to {forgetPasswordRequest.Email}");
                throw;
            }

        }

        public async Task<object> resetPassword(ResetPassword resetPassword)
        {

            var (isValid, errors) = ValidationHelper.ValidateObject(resetPassword);

            if (!isValid)
            {
                throw new InvalidRequestedDataException((int)ErrorCodes.INVALID_REQUEST_DATA, errors);
            }

            //define application user
            ApplicationUser employee;

            try
            {
                employee = await _userManager.FindByEmailAsync(resetPassword.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while finding the user by {resetPassword?.Email}");
                throw;
            }

            if (employee == null)
            {
                throw new UserNotFoundException((int)ErrorCodes.INVALID_EMAIL, "Can not find an user by provided email");
            }

            try
            {
                // Decode token from URL-safe Base64
                var decodedPasswordResetTokenBytes = WebEncoders.Base64UrlDecode(resetPassword.PasswordResetToken);
                var decodedPasswordResetToken = Encoding.UTF8.GetString(decodedPasswordResetTokenBytes);

                var resetPassResult = await _userManager.ResetPasswordAsync(employee, decodedPasswordResetToken, resetPassword.NewPassword);

                if (!resetPassResult.Succeeded)
                {
                    var Errors = resetPassResult.Errors.Select(e => e.Description);
                    return new
                    {
                        message = $"Error : {Errors}",
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
                _logger.LogError(ex, "An error occured while resetting the password.");
                throw;
            }

        }
    }
}
