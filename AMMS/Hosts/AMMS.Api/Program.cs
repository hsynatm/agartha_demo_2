using AMMS.Api;
using AMMS.Api.OpenApi;
using AMMS.Infrastructure;
using AMMS.Infrastructure.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging();

    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            // Validasyon ApiBaseController.EnsureValidRequest() içinde yapılır;
            // [ApiController] otomatik 400 filtresi action'a girmeden isteği kesmesin.
            options.SuppressModelStateInvalidFilter = true;
        });
    if (builder.Environment.IsApiDocumentationEnabled())
    {
        builder.Services.AddAmmsOpenApi();
    }
    builder.Services.AddHealthChecks();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    builder.Services.AddAmmsModules(connectionString);

    var app = builder.Build();

    await app.ApplyDevelopmentMigrationsAsync();
    app.UseAmmsApiDocumentation();
    app.UseAmmsPipeline();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
