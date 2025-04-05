using System.ComponentModel.DataAnnotations;

namespace API.DTO.Account
{
    public class RegisterDto
    {
        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 20 characters long.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 20 characters long.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters long.")]
        public string Password { get; set; } = string.Empty;
 
    }
}
