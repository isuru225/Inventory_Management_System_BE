using System.Security.Claims;
using TaskNest.Frontend.Models;

namespace TaskNest.IServices
{
    public interface ITokenService
    {
        public TokenResult CreateToken(List<Claim> claims);
    }
}
