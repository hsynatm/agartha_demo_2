namespace UserManagement.Application.Dtos;

public sealed class CreateUserDto
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string OrganizationName { get; set; } = null!;

    public List<string> Roles { get; set; } = [];

    public List<string> RoleGroups { get; set; } = [];
}
