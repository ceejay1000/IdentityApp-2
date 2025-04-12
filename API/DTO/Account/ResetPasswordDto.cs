using System.ComponentModel.DataAnnotations;

public class ResetPasswordDto {

    [Required]
    public string Token { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string NewPassword { get; set; }
}