using AMMS.Infrastructure.Localization;
using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace AMMS.Infrastructure.Middleware
{

    public sealed class CultureMiddleware
    {
        private readonly RequestDelegate _next;

        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cultureName = JsonStringLocalizer.ResolveCultureFromAcceptLanguage(
                context.Request.Headers.AcceptLanguage.ToString());

            context.Items[CultureConstants.HttpContextItemKey] = cultureName;

            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await _next(context);
        }
    }




}
