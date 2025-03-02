using Microsoft.AspNetCore.Identity;
using TaskNest.Frontend.Models;
using TaskNest.FrontendModels;
using TaskNest.Models;

namespace TaskNest.IServices
{
    public interface IUserManagementService
    {
        public Task<TokenResult> Login(UserLogin userLogin);
        public Task<String> CreateRole(CreateRole createRole);
        public Task<RegisterResponse> RegisteredUser(UserRegisterInfo userRegisterInfo);
        public Task<ApplicationUser> GetEmployeeInfo(string userName);
        public Task<List<RegisteredUsersInfo>> GetRegisteredUser();
        public Task<object> DeleteRegisteredUser(String Id);
    }
}
