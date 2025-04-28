using Api;
using API.DTO.Account;
using API.Models;
using API.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private JwtService _jwtService;
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;

        private readonly EmailService emailService;

        private readonly IConfiguration configuration;

        private readonly ILogger logger;

        private readonly HttpClient _facebookHttpClient;


        public AccountController(ILogger<AccountController> logger, IConfiguration configuration, EmailService emailService, JwtService jwtService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this._jwtService = jwtService;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailService = emailService;
            this.configuration = configuration;
            this.logger = logger;
            _facebookHttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://graph.facebook.com")
            };
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

            if (result.IsLockedOut)
            {
                return Unauthorized($"User is locked out until {user.LockoutEnd}");
            }

            if (!result.Succeeded)
            {

                if (user.UserName != "admin")
                {
                    await userManager.AccessFailedAsync(user);
                    if (user.AccessFailedCount >= 3)
                    {
                        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddDays(1));
                        return Unauthorized("Too many failed attempts. User is locked out. Please try again after 24 hours");
                    }
                }
            }

            if (result.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                await userManager.SetLockoutEndDateAsync(user, null);
            }
            var userDto = CreateApplicationUserDto(user);
            return Ok(userDto);
        }

        [HttpPost("login-with-third-party")]
        public async Task<ActionResult<UserDto>> LoginWithThirdParty(LoginWithExternalDto model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to login with facebook");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to login with facebook");
                }
            }
            else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to login with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to login with google");
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            var user = await userManager.Users.FirstOrDefaultAsync(x => x.UserName == model.UserId && x.Provider == model.Provider);
            if (user == null) return Unauthorized("Unable to find your account");

            return await CreateApplicationUserDto(user);
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
                // EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            await userManager.AddToRoleAsync(newUser, SD.Player);

            try
            {
                if (await SendConfirmEmailAsync(newUser))
                {
                    logger.LogInformation("Email sent successfully");
                    logger.LogInformation("User created successfully");

                    return Ok(new JsonResult(new { Title = "Account Created", message = "Your account has been created, please confirm your email address" }));
                }

                return BadRequest("Failed to send email. Please contact admin");

            }
            catch (Exception ex)
            {
                this.logger.LogError($"Email send error {ex.Message}");
                this.logger.LogError(ex.StackTrace.ToString());
                return BadRequest("Failed to send email. Please contact admin");
            }

        }


        [HttpPost("register-with-third-party")]
        public async Task<ActionResult<UserDto>> RegisterWithThirdParty(RegisterWithExternal model)
        {
            if (model.Provider.Equals(SD.Facebook))
            {
                try
                {
                    if (!FacebookValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to register with facebook");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to register with facebook");
                }
            }
            else if (model.Provider.Equals(SD.Google))
            {
                try
                {
                    if (!GoogleValidatedAsync(model.AccessToken, model.UserId).GetAwaiter().GetResult())
                    {
                        return Unauthorized("Unable to register with google");
                    }
                }
                catch (Exception)
                {
                    return Unauthorized("Unable to register with google");
                }
            }
            else
            {
                return BadRequest("Invalid provider");
            }

            var user = await userManager.FindByNameAsync(model.UserId);
            if (user != null) return BadRequest(string.Format("You have an account already. Please login with your {0}", model.Provider));

            var userToAdd = new User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.UserId,
                Provider = model.Provider,
            };

            var result = await userManager.CreateAsync(userToAdd);

            if (!result.Succeeded) return BadRequest(result.Errors);
            await userManager.AddToRoleAsync(userToAdd, SD.Player);

            return await CreateApplicationUserDto(userToAdd);
        }


        [HttpPut("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            var user = await userManager.FindByEmailAsync(confirmEmailDto.Email);

            if (user == null) return Unauthorized("This email has not been registered yet");

            if (user.EmailConfirmed == true) return BadRequest("Your email was confirmed before. Please login to your account");

            try
            {
                var docodedTokenBytes = WebEncoders.Base64UrlDecode(confirmEmailDto.Token);
                var decodedToken = Encoding.UTF8.GetString(docodedTokenBytes);
                var result = await userManager.ConfirmEmailAsync(user, decodedToken);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Email confirmed", message = "Your email address is confirmed. You can login now" }));
                }

                return BadRequest("Invalid token please try again");

            }
            catch (Exception ex)
            {
                return BadRequest("Invalid token please try again");
            }
        }

        [HttpPost("resend-email-confirmation-link/{email}")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest("Invalid email");
            var user = await userManager.FindByEmailAsync(email);

            if (user == null) return Unauthorized("This email has not been registered, pleaser register");

            if (user.EmailConfirmed == true) return BadRequest("Your email address was confirmed before. Pleade login to your account");

            try
            {
                if (await SendConfirmEmailAsync(user))
                {
                    logger.LogInformation("Email sent successfully");
                    logger.LogInformation("User created successfully");
                }

                return BadRequest("Failed to send email");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Email send error {ex.Message}");
                this.logger.LogError(ex.StackTrace.ToString());
                return BadRequest("Failed to send email. Please contact admin");
            }

            // return Ok(new JsonResult(new {Title = "Account Created", message = "Your account has been created, please confirm your email address"}));

        }

        [HttpPost("forgot-username-or-password/{email}")]
        public async Task<IActionResult> ForgotUsernameOrPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid Email");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized("This email has not been registered");
            }

            if (user.EmailConfirmed == true) return BadRequest("Your email address was confirmed before. Pleade login to your account");
            else if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address");

            try
            {
                if (await SendForgotUsernameOrPassword(user))
                {
                    return Ok(new JsonResult(new { title = "Password reset link sent", message = "Kindly check your mail for a password reset link" }));
                }

                return BadRequest("Failed to send email. PLease contact admin");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send email. Please contact admin");
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("This email address has not been registered");

            if (user.EmailConfirmed == false) return BadRequest("Please confirm your email address first");

            try
            {
                var docodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                var decodedToken = Encoding.UTF8.GetString(docodedTokenBytes);
                var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new JsonResult(new { title = "Password reset success", message = "Password has been reset successfully" }));
                }

                return BadRequest("Invalid token please try again");
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid token please try again");
            }
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
        private async Task<UserDto> CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = await _jwtService.CreateJwt(user)
            };
        }

        private async Task<bool> SendForgotUsernameOrPassword(User user)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ResetPasswordPath"]}?token={token}&email={user.Email}";
            var body = $"<p>Hello {user.FirstName} {user.LastName}</p>" +
            $"<p>Username: {user.UserName}</p>" +
            $"<p>In order to reset your password, please click on the following link</p>" +
            $"<p><a href=\"{url}\"></a><Click here/p>" +
            "<p> Thank you </p>" +
            $"<br>{configuration["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email!, "Confirm Your Email", body);
            logger.LogInformation("Sending email to user");
            return await emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> SendConfirmEmailAsync(User user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var url = $"{configuration["JWT:ClientUrl"]}/{configuration["Email:ConfirmEmailPath"]}?token={token}&email={user.Email}";
            var body = $"<p>Hello {user.FirstName} {user.LastName}</p>" +
                "<p> Please confirm your email address by clicking on the following link</p>" +
                $"<p><a href=\"{url}\"></a><Click here/p>" +
                "<p> Thank you </p>" +
                $"<br>{configuration["Email:ApplicationName"]}";

            var emailSend = new EmailSendDto(user.Email!, "Confirm Your Email", body);
            logger.LogInformation("Sending email to user");
            return await emailService.SendEmailAsync(emailSend);
        }

        private async Task<bool> FacebookValidatedAsync(string accessToken, string userId)
        {
            var facebookKeys = configuration["Facebook:AppId"] + "|" + configuration["Facebook:AppSecret"];
            var fbResult = await _facebookHttpClient.GetFromJsonAsync<FacebookResultDto>($"debug_token?input_token={accessToken}&access_token={facebookKeys}");

            if (fbResult == null || fbResult.Data.Is_Valid == false || !fbResult.Data.User_Id.Equals(userId))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> GoogleValidatedAsync(string accessToken, string userId)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken);

            if (!payload.Audience.Equals(configuration["Google:ClientId"]))
            {
                return false;
            }

            if (!payload.Issuer.Equals("accounts.google.com") && !payload.Issuer.Equals("https://accounts.google.com"))
            {
                return false;
            }

            if (payload.ExpirationTimeSeconds == null)
            {
                return false;
            }

            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime expiration = DateTimeOffset.FromUnixTimeSeconds((long)payload.ExpirationTimeSeconds).DateTime;
            if (now > expiration)
            {
                return false;
            }

            if (!payload.Subject.Equals(userId))
            {
                return false;
            }

            return true;
        }
        #endregion
    }

}
