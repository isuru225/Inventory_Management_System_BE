using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskNest.Config.Models;
using TaskNest.Frontend.Models;
using TaskNest.IServices;

namespace TaskNest.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly JWTSettings _jwtSettings;
        public TokenService(IConfiguration configuration, IOptions<JWTSettings> options)
        {
            _configuration = configuration;
            _jwtSettings = options.Value;
        }

        public TokenResult CreateToken(List<Claim> claims)
        {

            //Secret key which will be used later during validation
            string key = _jwtSettings.Key;
            var issuer = _jwtSettings.Issuer;  //normally this will be your site URL    

            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Key")));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            //Debug.WriteLine("CVCV",name);
               
            var token = new JwtSecurityToken(issuer, //Issure    
                            issuer,  //Audience    
                            claims,
                            expires: DateTime.Now.AddHours(5),
                            signingCredentials: credentials);
            var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenResult 
            { 
                AccessToken = jwt_token,
            };
        }
    }
}
