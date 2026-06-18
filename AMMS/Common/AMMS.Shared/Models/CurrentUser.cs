namespace AMMS.Shared.Models
{
    public class CurrentUser
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public IReadOnlyList<string> Roles { get; init; } = [];
    }
}
