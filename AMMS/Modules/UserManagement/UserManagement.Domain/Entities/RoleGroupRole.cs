namespace UserManagement.Domain.Entities;

public sealed class RoleGroupRole
{
    public Guid RoleGroupId { get; set; }

    public RoleGroup RoleGroup { get; set; } = null!;

    public Guid RoleId { get; set; }

    public Role Role { get; set; } = null!;
}
