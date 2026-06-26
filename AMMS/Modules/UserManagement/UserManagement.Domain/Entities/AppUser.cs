using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;

namespace UserManagement.Domain.Entities;

public sealed class AppUser : BaseEntity, Audit.IAuditableEntity
{
    public string? KeycloakUserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string OrganizationId { get; set; } = null!;

    public string OrganizationName { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<UserRoleGroup> UserRoleGroups { get; set; } = [];
}
