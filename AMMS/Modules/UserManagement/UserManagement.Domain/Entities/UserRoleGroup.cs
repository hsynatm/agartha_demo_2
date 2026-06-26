namespace UserManagement.Domain.Entities;

public sealed class UserRoleGroup
{
    public Guid UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public Guid RoleGroupId { get; set; }

    public RoleGroup RoleGroup { get; set; } = null!;
}
