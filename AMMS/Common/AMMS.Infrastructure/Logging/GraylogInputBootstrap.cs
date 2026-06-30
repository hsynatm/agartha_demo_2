using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AMMS.Infrastructure.Logging;

/// <summary>
/// Ensures the GELF TCP input required by Serilog exists in Graylog on API startup (dev).
/// </summary>
public sealed class GraylogInputBootstrap
{
    private const string RequiredInputTitle = "GELF TCP AMMS";
    private const string GelfTcpInputType = "org.graylog2.inputs.gelf.tcp.GELFTCPInput";

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.Zero,
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(10),
        TimeSpan.FromSeconds(20),
        TimeSpan.FromSeconds(30)
    ];

    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GraylogInputBootstrap> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GraylogInputBootstrap(
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<GraylogInputBootstrap> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (!_configuration.GetValue("Graylog:Enabled", true))
        {
            return;
        }

        _logger.LogInformation(
            "Graylog logging enabled. Target={Host}:{Port} Transport={Transport}",
            _configuration["Graylog:HostnameOrAddress"] ?? "127.0.0.1",
            _configuration.GetValue("Graylog:Port", 12201),
            _configuration["Graylog:TransportType"] ?? "Tcp");

        if (!_configuration.GetValue("Graylog:EnsureInputOnStartup", _environment.IsDevelopment()))
        {
            return;
        }

        _logger.LogInformation("Graylog input bootstrap started.");

        foreach (var delay in RetryDelays)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation(
                    "Retrying Graylog input bootstrap in {DelaySeconds}s.",
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }

            if (await TryEnsureInputAsync(cancellationToken))
            {
                return;
            }
        }

        _logger.LogError(
            "Graylog input bootstrap failed after {AttemptCount} attempts. " +
            "Ensure Graylog setup wizard is complete, then restart AMMS.Api.",
            RetryDelays.Length);
    }

    private async Task<bool> TryEnsureInputAsync(CancellationToken cancellationToken)
    {
        var graylogUrl = _configuration["Graylog:AdminUrl"] ?? "http://localhost:9000";
        var username = _configuration["Graylog:AdminUsername"] ?? "admin";
        var password = _configuration["Graylog:AdminPassword"] ?? "admin";
        var port = _configuration.GetValue("Graylog:Port", 12201);

        try
        {
            var client = _httpClientFactory.CreateClient();
            var authHeader = CreateAuthHeader(username, password);

            if (await InputExistsAsync(client, graylogUrl, authHeader, cancellationToken))
            {
                _logger.LogInformation("Graylog input '{InputTitle}' is ready.", RequiredInputTitle);
                return true;
            }

            await CreateInputAsync(client, graylogUrl, authHeader, port, cancellationToken);
            _logger.LogInformation(
                "Created Graylog input '{InputTitle}' on port {Port}.",
                RequiredInputTitle,
                port);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Graylog input bootstrap attempt failed.");
            return false;
        }
    }

    private async Task<bool> InputExistsAsync(
        HttpClient client,
        string graylogUrl,
        AuthenticationHeaderValue authHeader,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{graylogUrl.TrimEnd('/')}/api/system/inputs");
        request.Headers.Authorization = authHeader;
        request.Headers.Add("X-Requested-By", "amms-api");

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Graylog inputs query failed with HTTP {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("inputs", out var inputs))
        {
            return false;
        }

        foreach (var input in inputs.EnumerateArray())
        {
            if (input.TryGetProperty("title", out var title)
                && string.Equals(title.GetString(), RequiredInputTitle, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task CreateInputAsync(
        HttpClient client,
        string graylogUrl,
        AuthenticationHeaderValue authHeader,
        int port,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            title = RequiredInputTitle,
            type = GelfTcpInputType,
            global = true,
            configuration = new
            {
                bind_address = "0.0.0.0",
                port,
                recv_buffer_size = 262144,
                tcp_keepalive = false,
                use_null_delimiter = true,
                max_message_size = 2097152,
                decompress_size_limit = 8388608
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{graylogUrl.TrimEnd('/')}/api/system/inputs");
        request.Headers.Authorization = authHeader;
        request.Headers.Add("X-Requested-By", "amms-api");
        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Graylog input create failed with HTTP {(int)response.StatusCode}: {body}");
        }
    }

    private static AuthenticationHeaderValue CreateAuthHeader(string username, string password) =>
        new(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
}
