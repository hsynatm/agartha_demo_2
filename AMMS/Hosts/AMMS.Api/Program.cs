using AMMS.Api;
using AMMS.Api.Json;
using AMMS.Api.Swagger;
using AMMS.Infrastructure;
using AMMS.Infrastructure.Logging;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilogLogging();

    builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true; //model valid olmasa bile,controllere git,kendim kontrol edeceğim
    }).AddAmmsJsonOptions();

    if (builder.Environment.IsApiDocumentationEnabled())
    {
        builder.Services.AddAmmsSwagger();
    }
    builder.Services.AddHealthChecks();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

    builder.Services.AddAmmsModules(connectionString, builder.Configuration);

    var app = builder.Build();

    app.UseAmmsApiDocumentation();
    app.UseAmmsPipeline();
    app.UseCors("Spa");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health").AllowAnonymous();

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
