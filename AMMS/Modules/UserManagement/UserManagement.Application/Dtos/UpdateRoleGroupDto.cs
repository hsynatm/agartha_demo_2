namespace UserManagement.Application.Dtos;

public sealed class UpdateRoleGroupDto
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> Roles { get; set; } = [];
}
