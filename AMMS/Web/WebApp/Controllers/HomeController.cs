using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private const string SessionTerminatedCode = "SESSION_TERMINATED";
        private const string SessionIdleExpiredCode = "SESSION_IDLE_EXPIRED";
        private const string SessionIdleExpiredMessage = "Uzun süre işlem yapmadığınız için oturumunuz sonlandı. Lütfen tekrar giriş yapın.";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly KeycloakSessionService _keycloakSessionService;
        private readonly string baseUrlApi = "http://localhost:5121/api/v1/";
        private string GetClientId() => _configuration["Keycloak:ClientId"] ?? "amms-spa";

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration, KeycloakSessionService keycloakSessionService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _keycloakSessionService = keycloakSessionService;
        }


        public IActionResult Index()
        {
            var idleMinutes = _configuration.GetValue("Session:IdleTimeoutMinutes", 5);
            var graceMinutes = _configuration.GetValue("Session:IdleGraceMinutes", 2);
            ViewData["SessionIdleMs"] = (idleMinutes + graceMinutes) * 60_000;
            ViewData["SessionCheckIntervalMs"] = _configuration.GetValue("Session:CheckIntervalSeconds", 30) * 1_000;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckSession([FromHeader(Name = "Authorization")] string? authorization, CancellationToken cancellationToken)
        {
            var accessToken = ExtractBearerToken(authorization);
            if (accessToken is null)
            {
                return SessionEndedResult(SessionIdleExpiredCode, SessionIdleExpiredMessage);
            }

            var active = await _keycloakSessionService.IsAccessTokenActiveAsync(accessToken, cancellationToken);
            if (!active)
            {
                return SessionEndedResult(SessionIdleExpiredCode, SessionIdleExpiredMessage);
            }

            return Json(new { success = true, active = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Login(string pUserName, string password, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(pUserName) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { success = false, message = "Username and password are required." });
            }

            await TryRevokeSessionsBeforeLoginAsync(pUserName, cancellationToken);

            var (ok, body, statusCode) = await RequestKeycloakTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = GetClientId(),
                ["username"] = pUserName,
                ["password"] = password
            }, cancellationToken);

            if (!ok)
            {
                return StatusCode(statusCode, new
                {
                    success = false,
                    message = BuildLoginFailureMessage(body),
                    detail = body
                });
            }

            var tokens = ParseTokenResponse(body);
            if (tokens is null)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { success = false, message = "Invalid token response." });
            }

            return Json(new
            {
                success = true,
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken
            });
        }

        [HttpGet]
        public Task<IActionResult> SorgulaAsset(string id, [FromHeader(Name = "Authorization")] string? authorization, [FromHeader(Name = "X-Refresh-Token")] string? refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Task.FromResult<IActionResult>(BadRequest(new { success = false, message = "Asset id is required." }));
            }

            var accessToken = ExtractBearerToken(authorization);
            if (accessToken is null)
            {
                return Task.FromResult<IActionResult>(Unauthorized(new { success = false, message = "Access token is required. Login first." }));
            }

            return ForwardAuthorizedApiAsync(
                accessToken,
                refreshToken,
                token => CallAssetApiAsync(id, token, cancellationToken),
                "Asset query failed.",
                cancellationToken);
        }

        [HttpPost]
        public Task<IActionResult> CreateUser(string name, string password, [FromHeader(Name = "Authorization")] string? authorization, [FromHeader(Name = "X-Refresh-Token")] string? refreshToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult<IActionResult>(BadRequest(new { success = false, message = "Name and password are required." }));
            }

            var accessToken = ExtractBearerToken(authorization);
            if (accessToken is null)
            {
                return Task.FromResult<IActionResult>(Unauthorized(new { success = false, message = "Access token is required. Login first." }));
            }

            var createUserJson = BuildCreateUserJson(name, password);

            return ForwardAuthorizedApiAsync(
                accessToken,
                refreshToken,
                token => CallCreateUserApiAsync(createUserJson, token, cancellationToken),
                "Create user failed.",
                cancellationToken);
        }

        private async Task<IActionResult> ForwardAuthorizedApiAsync(string accessToken, string? refreshToken, Func<string, Task<(int StatusCode, string Body)>> callApi, string failureMessage, CancellationToken cancellationToken)
        {
            var (statusCode, responseBody) = await callApi(accessToken);
            var tokensUpdated = false;

            if (statusCode == StatusCodes.Status401Unauthorized && IsSessionEnded(responseBody))
            {
                return SessionEndedContent(responseBody);
            }

            if (statusCode == StatusCodes.Status401Unauthorized && !string.IsNullOrWhiteSpace(refreshToken))
            {
                var newTokens = await RefreshTokensAsync(refreshToken, cancellationToken);
                if (newTokens is null)
                {
                    return SessionEndedResult(SessionIdleExpiredCode, SessionIdleExpiredMessage);
                }

                accessToken = newTokens.AccessToken;
                refreshToken = newTokens.RefreshToken ?? refreshToken;
                tokensUpdated = true;
                (statusCode, responseBody) = await callApi(accessToken);

                if (statusCode == StatusCodes.Status401Unauthorized && IsSessionEnded(responseBody))
                {
                    return SessionEndedContent(responseBody);
                }
            }

            if (statusCode is not StatusCodes.Status200OK and not StatusCodes.Status201Created)
            {
                return StatusCode(statusCode, new
                {
                    success = false,
                    message = failureMessage,
                    statusCode,
                    detail = ParseJsonOrString(responseBody)
                });
            }

            return Json(new
            {
                success = true,
                tokensUpdated,
                accessToken = tokensUpdated ? accessToken : null,
                refreshToken = tokensUpdated ? refreshToken : null,
                result = ParseJsonOrString(responseBody)
            });
        }

        private async Task<(int StatusCode, string Body)> CallAssetApiAsync(string id, string accessToken, CancellationToken cancellationToken)
        {
            var apiBaseUrl = _configuration["Api:BaseUrl"] ?? baseUrlApi;
            var url = $"{apiBaseUrl.TrimEnd('/')}/asset-management/getbyid/{id}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await SendApiRequestAsync(request, cancellationToken);
        }

        private async Task<(int StatusCode, string Body)> CallCreateUserApiAsync(string jsonBody, string accessToken, CancellationToken cancellationToken)
        {
            var apiBaseUrl = _configuration["Api:BaseUrl"] ?? baseUrlApi;
            var url = $"{apiBaseUrl.TrimEnd('/')}/user-management/Create";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            return await SendApiRequestAsync(request, cancellationToken);
        }

        private async Task<(int StatusCode, string Body)> SendApiRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ((int)response.StatusCode, body);
        }

        private static string BuildCreateUserJson(string username, string password)
        {
            return JsonSerializer.Serialize(new
            {
                username,
                password,
                email = $"{username}@gmail.com",
                firstName = "string",
                lastName = "string",
                organizationId = "string",
                organizationName = "string",
                roles = new[] { "test_rol1" },
                roleGroups = new[] { "UserManagement" }
            });
        }


        private async Task<TokenPair?> RefreshTokensAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var (ok, body, _) = await RequestKeycloakTokenAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = GetClientId(),
                ["refresh_token"] = refreshToken
            }, cancellationToken);

            return ok ? ParseTokenResponse(body) : null;
        }

        private async Task<(bool Ok, string Body, int StatusCode)> RequestKeycloakTokenAsync( Dictionary<string, string> formData, CancellationToken cancellationToken)
        {
            var authority = _configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/amms";
            var tokenUrl = $"{authority.TrimEnd('/')}/protocol/openid-connect/token";

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(formData)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return (response.IsSuccessStatusCode, body, (int)response.StatusCode);
        }


        private static TokenPair? ParseTokenResponse(string responseBody)
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (!root.TryGetProperty("access_token", out var accessTokenElement))
            {
                return null;
            }

            var accessToken = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            root.TryGetProperty("refresh_token", out var refreshTokenElement);
            return new TokenPair(accessToken, refreshTokenElement.GetString());
        }

        private static string? ExtractBearerToken(string? authorization)
        {
            if (string.IsNullOrWhiteSpace(authorization)
                || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorization["Bearer ".Length..].Trim();
        }

        private static ContentResult SessionEndedResult(string code, string message) =>
            SessionEndedContent(JsonSerializer.Serialize(new { code, message }));

        private static ContentResult SessionEndedContent(string responseBody) =>
            new()
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = responseBody,
                ContentType = "application/json"
            };

        private static bool IsSessionEnded(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(responseBody);
                if (!document.RootElement.TryGetProperty("code", out var codeElement))
                {
                    return false;
                }

                var code = codeElement.GetString();
                return string.Equals(code, SessionTerminatedCode, StringComparison.Ordinal)
                       || string.Equals(code, SessionIdleExpiredCode, StringComparison.Ordinal);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static object ParseJsonOrString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            try
            {
                return JsonSerializer.Deserialize<JsonElement>(value)!;
            }
            catch (JsonException)
            {
                return value;
            }
        }

        private async Task TryRevokeSessionsBeforeLoginAsync(string username, CancellationToken cancellationToken)
        {
            try
            {
                await _keycloakSessionService.RevokeAllSessionsByUsernameAsync(username, cancellationToken);
            }
            catch (Exception)
            {
                // Login should not fail when admin session revoke is unavailable.
            }
        }

        private static string BuildLoginFailureMessage(string keycloakBody)
        {
            if (keycloakBody.Contains("invalid_grant", StringComparison.OrdinalIgnoreCase)
                && keycloakBody.Contains("Invalid user credentials", StringComparison.OrdinalIgnoreCase))
            {
                return "Keycloak kullanici adi veya sifre hatali. Keycloak sifresi kullanici adi ile aynidir.";
            }

            return "Keycloak token request failed.";
        }

        private sealed record TokenPair(string AccessToken, string? RefreshToken);
    }
}
