using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Principal;
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
        public UserManagementService
            (
            ILogger<UserManagementService> logger,
            IConfiguration configuration,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _logger = logger;
            _configuration = configuration;
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
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

                        throw new InvalidCredentialException("Could not sign in");
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
                    throw new UserNotFoundException("Can not find an user by provided email");
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

                if (employee != null)
                {

                }
                else
                {
                    throw new UserNotFoundException("Can not find an user by provided email");
                }

                return employee;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,"An error occured while getting employee info from Application User");
                throw ex;
            }
        }
    }
}
