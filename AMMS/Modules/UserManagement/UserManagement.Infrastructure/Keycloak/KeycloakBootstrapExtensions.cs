using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UserManagement.Infrastructure.Keycloak;

public static class KeycloakBootstrapExtensions
{
    public static async Task RunUserManagementBootstrapAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AdminUserBootstrapRunner>().RunAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<KeycloakUserBootstrapRunner>().RunAsync(cancellationToken);
    }

    public static async Task RunKeycloakUserBootstrapAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<KeycloakUserBootstrapRunner>();
        await runner.RunAsync(cancellationToken);
    }
}
