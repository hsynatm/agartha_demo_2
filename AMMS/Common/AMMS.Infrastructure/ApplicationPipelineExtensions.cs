using AMMS.Infrastructure.Logging;
using AMMS.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace AMMS.Infrastructure
{

    public static class ApplicationPipelineExtensions
    {
        /// <summary>
        /// AMMS HTTP pipeline'ını doğru sırayla kaydeder:
        /// CorrelationId → Culture → LogContext → RequestLogging → ExceptionHandling
        /// ExceptionHandling controller'a yakın olmalı; request log yanıt yazıldıktan sonra düşer.
        /// </summary>
        public static WebApplication UseAmmsPipeline(this WebApplication app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<CultureMiddleware>();
            app.UseMiddleware<LogContextMiddleware>();
            app.UseAmmsRequestLogging();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }



}
