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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayController : ControllerBase
    {
        private JwtService _jwtService;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        public PlayController(JwtService jwtService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this._jwtService = jwtService;
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        [HttpPost("get-players")]
        public IActionResult Players()
        {
            return Ok(new JsonResult(new {message = "Only Authorized users can view players"}));
        }
    }
}

