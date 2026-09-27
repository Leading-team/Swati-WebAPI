using SWWebAPI.Data;
using SWWebAPI.Models.Api;
using SWWebAPI.Handler;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SWWebAPI.Services
{
    public class JwtService
    {
       private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public JwtService(ApplicationDbContext dbContext,IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;

        }
        public async Task<LoginResponseModel?> Authenticate(LoginRequestModel request)
        { 
          if(string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password)) 
          {
            return null;
          }
            var userAccount = await _dbContext.UserAccounts.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if(userAccount is null || !PasswordHashHandler.VerifyPassword(request.Password, userAccount.Password))
            {
                return null;
            }
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = _configuration["Jwt:Key"];
            var tokenValidityMins=_configuration.GetValue<int>("Jwt:TokenExpirationInMinutes");
            var tokenExpiryTimeStampe = DateTime.Now.AddMinutes(tokenValidityMins);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Name, request.UserName)
                   
                }),
                Expires = tokenExpiryTimeStampe,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)), SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);
            return new LoginResponseModel
            {
                UserName = request.UserName,
                AccessToken = accessToken,
                ExpiresIn = (int)tokenExpiryTimeStampe.Subtract(DateTime.Now).TotalSeconds
            };

        }
    }
}
