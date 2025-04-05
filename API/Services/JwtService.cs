using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services
{
    public class JwtService
    {
        private IConfiguration _configuration;
        private UserManager<User> _userManager;
        private readonly SymmetricSecurityKey _jwtKey;

        public JwtService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        }

        public string CreateJwt(User user)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),          
                new Claim(ClaimTypes.Email, user.Email!),          
                new Claim(ClaimTypes.GivenName, user.FirstName),  
                new Claim(ClaimTypes.Surname, user.LastName),
            };

            var credentials = new SigningCredentials(_jwtKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
               // Audience = _configuration["Jwt:Audience"]
            };  

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(jwt);
        }
    }
}
