using AMMS.Infrastructure.Logging;
using AMMS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AMMS.Infrastructure
{

    public static class ApplicationPipelineExtensions
    {
        /// <summary>
        /// AMMS HTTP pipeline'ını doğru sırayla kaydeder:
        /// CorrelationId (istek takip numarası) → Culture → LogContext → ExceptionHandling → RequestLogging
        /// </summary>
        public static WebApplication UseAmmsPipeline(this WebApplication app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<CultureMiddleware>();
            app.UseMiddleware<LogContextMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAmmsRequestLogging();
            return app;
        }
    }



}
