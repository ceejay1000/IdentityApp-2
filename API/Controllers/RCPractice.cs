using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class RCPractice : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Context _context;

        public RCPractice(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Public");
        }

        [HttpGet("admin-role")]
        [Authorize(Roles = SD.Admin)]
        public IActionResult AdminRole()
        {
            return Ok("Admin Role");
        }

        [HttpGet("manager-role")]
        [Authorize(Roles = SD.Manager)]
        public IActionResult ManagerROle()
        {
            return Ok("Manager Role");
        }

        [HttpGet("admin-manager-role")]
        [Authorize(Roles = $"{SD.Admin},{SD.Manager}")]
        public IActionResult AdminOrManager()
        {
            return Ok("Admin or Manager Role");
        }

        [HttpGet("player-role")]
        [Authorize(Roles = SD.Player)]
        public IActionResult Player()
        {
            return Ok("Player Role");
        }

        #region Policies
        [HttpGet("admin-policy")]
        [Authorize(Policy = "Admin")]
        public IActionResult AdminPolicy()
        {
            return Ok("Admin Policy");
        }

        [HttpGet("manager-policy")]
        [Authorize(Policy = "Manager")]
        public IActionResult ManagerPolicy()
        {
            return Ok("Manager Policy");
        }

        [HttpGet("admin-or-manager-policy")]
        [Authorize(Policy = "RequireAdminOrManagerRole")]
        public IActionResult AdminOrManagerPolicy()
        {
            return Ok("Admin or Manager Policy");
        }
        #endregion

        #region ClaimsPolicy
        [HttpGet("admin-email-policy")]
        [Authorize(Policy = "AdminEmailPolicy")]
        public IActionResult AdminEmailPolicy()
        {
            return Ok("Admin Email Policy");
        }

        [HttpGet("manager-name-policy")]
        [Authorize(Policy = "managerNamePolicy")]
        public IActionResult ManagerNamePolicy()
        {
            return Ok("Manager Name Policy");
        }
        #endregion

        #region VIPPolicy
        [HttpGet("vip-policy")]
        [Authorize(Policy = "VIPPolicy")]
        public IActionResult VIPPolicy()
        {
            return Ok("VIP Policy");
        }
        #endregion
    }


}