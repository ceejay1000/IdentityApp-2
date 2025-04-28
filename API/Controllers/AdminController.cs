using Api;
using API.Data;
using API.DTOs.Admin;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Controllers


{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Context _context;

        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet("get-members")]
        public async Task<ActionResult<IEnumerable<MemberViewDto>>> GetMembers()
        {
            var members = await _userManager.Users.Where(x => x.UserName != "admin@role.com").Select(x => new MemberViewDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.Email!,
                DateCreated = x.DateCreated,
                Provider = x.Provider,
                IsLocked = _userManager.IsLockedOutAsync(x).GetAwaiter().GetResult(),
                Roles = _userManager.GetRolesAsync(x).GetAwaiter().GetResult()
            }).ToListAsync();

            return Ok(members);
        }

        [HttpPut("lock-member/{id}")]
        public async Task<ActionResult> LockMember(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member == null) return NotFound("Member not found");

            if (await IsUserInRole(member.Id, SD.Admin)) return BadRequest("Cannot lock admin");

            var result = await _userManager.SetLockoutEndDateAsync(member, DateTimeOffset.UtcNow.AddDays(5));
            if (!result.Succeeded) return BadRequest("Failed to lock member");

            return NoContent();
        }

        [HttpPut("unlock-member/{id}")]
        public async Task<ActionResult> UnlockMember(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member == null) return NotFound("Member not found");

            if (await IsUserInRole(member.Id, SD.Admin)) return BadRequest("Cannot unlock admin");

            var result = await _userManager.SetLockoutEndDateAsync(member, DateTimeOffset.UtcNow);
            if (!result.Succeeded) return BadRequest("Failed to unlock member");

            return NoContent();
        }

        [HttpDelete("delete-member/{id}")]
        public async Task<ActionResult> DeleteMember(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member == null) return NotFound("Member not found");

            if (await IsUserInRole(member.Id, SD.Admin)) return BadRequest("Cannot delete admin");

            var result = await _userManager.DeleteAsync(member);
            if (!result.Succeeded) return BadRequest("Failed to delete member");

            return NoContent();
        }

        [HttpGet("get-roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync();
            return Ok(roles);
        }

        [HttpGet("get-member/{id}")]
        public async Task<ActionResult<IEnumerable<MemberAddEditDto>>> GetMember(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member == null) return NotFound("Member not found");

            var existingMember = await _userManager.Users.Where(x => x.Id == id).Select(x => new MemberAddEditDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.Email!,
                Roles = string.Join(",", _userManager.GetRolesAsync(x).GetAwaiter().GetResult()),
            }).ToListAsync();
            return Ok(existingMember);
        }

        [HttpPost("add-member")]
        public async Task<ActionResult> AddMember(MemberAddEditDto memberDto)
        {
            var existingMember = await _userManager.FindByEmailAsync(memberDto.Id);
            if (existingMember != null) return BadRequest("Member already exists");

            var newMember = new User
            {
                FirstName = memberDto.FirstName,
                LastName = memberDto.LastName,
                UserName = memberDto.Username,
                Email = memberDto.Username,
                DateCreated = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(memberDto.Password))
            {
                if (memberDto.Password.Length < 6)
                {
                    return BadRequest("Password must be at least 6 characters long");
                }

                if (IsAdminUserId(memberDto.Id))
                {
                    return BadRequest("Cannot create admin user");
                }

                var user = await _userManager.FindByIdAsync(memberDto.Id);
                if (user != null)
                {
                    return BadRequest("User already exists");
                }
                //    newMember.PasswordHash = _userManager.PasswordHasher.HashPassword(newMember, memberDto.Password);

                if (!string.IsNullOrEmpty(memberDto.Password))
                {
                    await _userManager.RemovePasswordAsync(newMember);
                    await _userManager.AddPasswordAsync(newMember, memberDto.Password);
                }

            }

            var result = await _userManager.CreateAsync(newMember, memberDto.Password);
            if (!result.Succeeded) return BadRequest("Failed to create member");

            var roles = memberDto.Roles.Split(",").ToList();
            await _userManager.AddToRolesAsync(newMember, roles);

            return NoContent();
        }

        private bool IsAdminUserId(string id)
        {
            return _userManager.Users.Any(x => x.Id == id);
        }

        private async Task<bool> IsUserInRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.IsInRoleAsync(user, role);
        }
    }
}