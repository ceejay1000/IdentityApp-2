using API.DTO.Account;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private JwtService _jwtService;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public AccountController(JwtService jwtService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this._jwtService = jwtService;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized("Email not confirmed");
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid password");
            }
            var userDto = CreateApplicationUserDto(user);
            return Ok(userDto);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await CheckEmailExistsAsync(model.Email))
            {
                return BadRequest("Email already exists");
            }

            var newUser = new User
            {
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                EmailConfirmed = true
            };   
            
            var result = await userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User created successfully");
        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
           // var user = await userManager.FindByNameAsync(User.Identity.Name);
            var user = await userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value!);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var userDto = CreateApplicationUserDto(user);
            return Ok(userDto);
        }

        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await userManager.Users.AnyAsync(x => x.Email == email);
        }

        #region Private Helper Methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = _jwtService.CreateJwt(user)
            };          
        }
        #endregion
    }
}
