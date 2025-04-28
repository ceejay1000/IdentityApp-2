namespace API.DTOs.Admin;

public class MemberAddEditDto
{

    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }

    public string Password { get; set; }

    public string Roles { get; set; } = string.Empty;
}
