using AMMS.Shared.Dtos;

namespace UserManagement.Application.Dtos;

public sealed class UserDto : BaseDto
{
    public string? KeycloakUserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string OrganizationName { get; set; } = null!;

    public List<string> Roles { get; set; } = [];

    public List<string> RoleGroups { get; set; } = [];

    public bool IsActive { get; set; }
}
