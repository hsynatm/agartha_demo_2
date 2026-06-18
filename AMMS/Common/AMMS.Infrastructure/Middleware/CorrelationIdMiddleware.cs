using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMMS.Infrastructure.Middleware
{
    public static class CorrelationIdConstants
    {
        /// İstek/yanıtta taşınan takip numarası header adı
        public const string HeaderName = "X-Correlation-ID";

        /// Pipeline boyunca CorrelationId'nin okunduğu anahtar
        public const string HttpContextItemKey = "CorrelationId";
    }


    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }











}
