using AMMS.Infrastructure.Logging;
using AMMS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AMMS.Infrastructure
{

    public static class ApplicationPipelineExtensions
    {
        /// <summary>
        /// AMMS HTTP pipeline'ını doğru sırayla kaydeder:
        /// CorrelationId → Culture → LogContext → RequestLogging
        /// ExceptionHandling, Program.cs içinde auth sonrası eklenmeli (controller'a yakın).
        /// </summary>
        public static WebApplication UseAmmsPipeline(this WebApplication app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<CultureMiddleware>();
            app.UseMiddleware<LogContextMiddleware>();
            app.UseAmmsRequestLogging();
            return app;
        }

        public static WebApplication UseAmmsExceptionHandling(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }



}
