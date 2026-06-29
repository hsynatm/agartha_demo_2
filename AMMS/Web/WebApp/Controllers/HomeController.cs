using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string baseUrlApi = "http://localhost:5121/api/v1/";

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
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
                    message = "Keycloak token request failed.",
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
        public async Task<IActionResult> SorgulaAsset( string id,[FromHeader(Name = "Authorization")] string? authorization,[FromHeader(Name = "X-Refresh-Token")] string? refreshToken,CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { success = false, message = "Asset id is required." });
            }

            var accessToken = ExtractBearerToken(authorization);
            if (accessToken is null)
            {
                return Unauthorized(new { success = false, message = "Access token is required. Login first." });
            }

            var (statusCode, responseBody) = await CallAssetApiAsync(id, accessToken, cancellationToken);
            var tokensUpdated = false;

            if (statusCode == StatusCodes.Status401Unauthorized && !string.IsNullOrWhiteSpace(refreshToken))
            {
                var newTokens = await RefreshTokensAsync(refreshToken, cancellationToken);                if (newTokens is null)
                
                    return Unauthorized(new { success = false, message = "Access token expired and refresh failed. Login again." });
                

                accessToken = newTokens.AccessToken;
                refreshToken = newTokens.RefreshToken ?? refreshToken;
                tokensUpdated = true;
                (statusCode, responseBody) = await CallAssetApiAsync(id, accessToken, cancellationToken);
            }

            if (statusCode != StatusCodes.Status200OK)
            {
                return StatusCode(statusCode, new
                {
                    success = false,
                    message = "Asset query failed.",
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

        [HttpPost]
        public IActionResult CreateUser(string pname, string password)
        {
            var url = baseUrlApi + "user-management/Create";

            return Json(new
            {
                action = nameof(CreateUser),
                userName = pname,
                requestUrl = url
            });
        }

        private async Task<(int StatusCode, string Body)> CallAssetApiAsync(string id,string accessToken, CancellationToken cancellationToken)
        {
            var apiBaseUrl = _configuration["Api:BaseUrl"] ?? baseUrlApi;
            var url = $"{apiBaseUrl.TrimEnd('/')}/asset-management/getbyid/{id}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ((int)response.StatusCode, body);
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

        private async Task<(bool Ok, string Body, int StatusCode)> RequestKeycloakTokenAsync(Dictionary<string, string> formData, CancellationToken cancellationToken)
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

        private string GetClientId() => _configuration["Keycloak:ClientId"] ?? "amms-spa";

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

        private sealed record TokenPair(string AccessToken, string? RefreshToken);
    }
}
