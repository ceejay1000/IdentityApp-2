using System.Data.Common;
using System.Security.Claims;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class ContextSeedService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Context _context;

        public ContextSeedService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, Context context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task InitializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Any())
            {
                await _context.Database.MigrateAsync();
            }

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.Admin });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.Manager });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.Player });
            }

            if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
            {

                var adminUser = new User
                {
                    FirstName = "admin",
                    LastName = "jackson",
                    UserName = "admin@role.com",
                    Email = "admin@role.com",

                };

                await _userManager.CreateAsync(adminUser, "Admin@123");
                await _userManager.AddToRolesAsync(adminUser, new string[] { SD.Admin, SD.Manager, SD.Player });
                await _userManager.AddClaimsAsync(adminUser, new Claim[]
                {
                    new Claim(ClaimTypes.Email, adminUser.Email),
                    new Claim(ClaimTypes.Surname, adminUser.LastName),

                });


                var manager = new User
                {
                    FirstName = "manager",
                    LastName = "wilnom",
                    UserName = "manager@role.com",
                    Email = "manager@role.com",

                };

                await _userManager.CreateAsync(manager, "manager@123");
                await _userManager.AddToRolesAsync(manager, new string[] { SD.Manager, SD.Player });
                await _userManager.AddClaimsAsync(manager, new Claim[]
                {
                    new Claim(ClaimTypes.Email, manager.Email),
                    new Claim(ClaimTypes.Surname, manager.LastName),

                });


                var player = new User
                {
                    FirstName = "player",
                    LastName = "Kelvin",
                    UserName = "player@role.com",
                    Email = "player@role.com",

                };

                await _userManager.CreateAsync(player, "player@123");
                await _userManager.AddToRolesAsync(player, new string[] { SD.Player });
                await _userManager.AddClaimsAsync(player, new Claim[]
                {
                    new Claim(ClaimTypes.Email, player.Email),
                    new Claim(ClaimTypes.Surname, player.LastName),
                });



                var vipPlayer = new User
                {
                    FirstName = "vipPlayer",
                    LastName = "Thompson",
                    UserName = "vipPlayer@role.com",
                    Email = "vipPlayer@role.com",

                };

                await _userManager.CreateAsync(vipPlayer, "vipPlayer@123");
                await _userManager.AddToRolesAsync(vipPlayer, new string[] { SD.Player });
                await _userManager.AddClaimsAsync(vipPlayer, new Claim[]
                {
                    new Claim(ClaimTypes.Email, vipPlayer.Email),
                    new Claim(ClaimTypes.Surname, vipPlayer.LastName),
                });
            }

        }
    }
}