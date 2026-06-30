using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace AMMS.Infrastructure.Logging;

internal static class GraylogSinkConfigurator
{
    public static LoggerConfiguration AddAmmsGraylog(this LoggerConfiguration loggerConfiguration,IConfiguration configuration, IHostEnvironment environment)
    {
        if (!configuration.GetValue("Graylog:Enabled", true))
        {
            return loggerConfiguration;
        }

        var hostname = configuration["Graylog:HostnameOrAddress"] ?? "127.0.0.1";
        var port = configuration.GetValue("Graylog:Port", 12201);
        var transportName = configuration["Graylog:TransportType"] ?? "Tcp";
        var minimumLevelName = configuration["Graylog:MinimumLevel"] ?? "Information";

        if (!Enum.TryParse(transportName, ignoreCase: true, out TransportType transportType))
        {
            transportType = TransportType.Tcp;
        }

        if (!Enum.TryParse(minimumLevelName, ignoreCase: true, out LogEventLevel minimumLevel))
        {
            minimumLevel = LogEventLevel.Information;
        }

        var options = new GraylogSinkOptions
        {
            HostnameOrAddress = hostname,
            Port = port,
            TransportType = transportType,
            Facility = environment.ApplicationName,
            MinimumLogEventLevel = minimumLevel
        };

        return loggerConfiguration.WriteTo.Graylog(options);
    }
}
