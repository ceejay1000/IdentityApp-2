namespace API.DTOs.Admin;

public class MemberViewDto
{
    internal string Provider;

    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public bool IsLocked { get; set; }
    public DateTime DateCreated { get; set; }

    public IEnumerable<string> Roles { get; set; }
}
