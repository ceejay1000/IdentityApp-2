using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Api
{
    public static class SD
    {
        public const string Facebook = "facebook";
        public const string Google = "google";

        // Roles
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Player = "Player";

        public static bool VIPPolicy(AuthorizationHandlerContext context)
        {
            if (context.User.IsInRole(Player) && context.User.HasClaim(c => c.Type == ClaimTypes.Email && c.Value.Contains("vip")))
            {
                return true;
            }
            return false;
        }
    }
}