using System.Text;
using System.Text.Json;

namespace WebApp.Services;

public static class SessionEndReasonResolver
{
    public const string SessionTerminatedCode = "SESSION_TERMINATED";
    public const string SessionIdleExpiredCode = "SESSION_IDLE_EXPIRED";

    public const string SessionTerminatedMessage =
        "Başka bir bilgisayarda oturum açtığınız için mevcut oturumunuz sonlandırıldı.";

    public const string SessionIdleExpiredMessage =
        "Uzun süre işlem yapmadığınız için oturumunuz sonlandı. Lütfen tekrar giriş yapın.";

    public static (string Code, string Message) ResolveInactiveToken(string accessToken) =>
        IsStillValidByClock(accessToken)
            ? (SessionTerminatedCode, SessionTerminatedMessage)
            : (SessionIdleExpiredCode, SessionIdleExpiredMessage);

    /// <summary>
    /// Token süresi dolmamış ama Keycloak introspection inactive → başka cihazda giriş.
    /// </summary>
    public static bool IsStillValidByClock(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        var parts = accessToken.Split('.');
        if (parts.Length < 2)
        {
            return false;
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var document = JsonDocument.Parse(payloadJson);
            if (!document.RootElement.TryGetProperty("exp", out var expElement))
            {
                return false;
            }

            var expUnix = expElement.ValueKind switch
            {
                JsonValueKind.Number => expElement.GetInt64(),
                JsonValueKind.String when long.TryParse(expElement.GetString(), out var parsed) => parsed,
                _ => 0L
            };

            if (expUnix <= 0)
            {
                return false;
            }

            return DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime > DateTime.UtcNow;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2:
                padded += "==";
                break;
            case 3:
                padded += "=";
                break;
        }

        return Convert.FromBase64String(padded);
    }
}
