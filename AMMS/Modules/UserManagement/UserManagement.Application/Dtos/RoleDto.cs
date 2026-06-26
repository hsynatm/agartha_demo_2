using AMMS.Shared.Dtos;

namespace UserManagement.Application.Dtos;

public sealed class RoleDto : BaseDto
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}
