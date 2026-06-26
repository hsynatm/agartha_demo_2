namespace UserManagement.Application.Dtos;

public sealed class CreateRoleGroupDto
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public List<string> Roles { get; set; } = [];
}
