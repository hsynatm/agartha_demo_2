using Microsoft.AspNetCore.Http;

namespace AMMS.Infrastructure.Authentication;

public static class SessionTerminatedContext
{
    public const string HttpContextItemKey = "Amms.SessionTerminated";

    public const string ErrorCode = "SESSION_TERMINATED";

    public const string Message = "Başka bir bilgisayarda oturum açtığınız için mevcut oturumunuz sonlandırıldı.";

    public static bool IsTerminated(HttpContext? context) =>
        context?.Items[HttpContextItemKey] is true;
}
