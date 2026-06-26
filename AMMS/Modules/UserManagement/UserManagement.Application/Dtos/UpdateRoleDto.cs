namespace UserManagement.Application.Dtos;

public sealed class UpdateRoleDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
