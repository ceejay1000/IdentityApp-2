using System.ComponentModel.DataAnnotations;

public class ConfirmEmailDto
{
    [Required]
    public string Token { get; set; }

    [Required]
    public string Email { get; set; }
}