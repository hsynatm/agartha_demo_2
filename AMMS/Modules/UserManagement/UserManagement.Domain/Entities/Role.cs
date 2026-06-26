using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;

namespace UserManagement.Domain.Entities;

public sealed class Role : BaseEntity, Audit.IAuditableEntity
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = [];

    public ICollection<RoleGroupRole> RoleGroupRoles { get; set; } = [];
}
