using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AMMS.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAmmsAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddHttpClient();
        services.AddSingleton<KeycloakTokenIntrospectionService>();

        services.Configure<AmmsAuthenticationOptions>(
            configuration.GetSection(AmmsAuthenticationOptions.SectionName));

        var authOptions = configuration
            .GetSection(AmmsAuthenticationOptions.SectionName)
            .Get<AmmsAuthenticationOptions>() ?? new AmmsAuthenticationOptions();

        if (string.IsNullOrWhiteSpace(authOptions.IntrospectionClientSecret))
        {
            authOptions.IntrospectionClientSecret = configuration[$"KeycloakAdmin:ClientSecret"] ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(authOptions.Authority))
        {
            throw new InvalidOperationException(
                $"Configuration section '{AmmsAuthenticationOptions.SectionName}:Authority' is required.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAudiences = new[] { authOptions.Audience, "account" },
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var authorizedParty = context.Principal?.FindFirst("azp")?.Value;
                        if (!string.IsNullOrWhiteSpace(authorizedParty)
                            && !string.Equals(authorizedParty, authOptions.SpaClientId, StringComparison.Ordinal))
                        {
                            AuthorizationFailureContext.Set(
                                context.HttpContext,
                                new AuthorizationFailureSnapshot(
                                    "Token authorized party is not allowed.",
                                    KeycloakClaims.GetKeycloakUserId(context.Principal)));
                            context.Fail("Token authorized party is not allowed.");
                            return;
                        }

                        if (!authOptions.EnableTokenIntrospection)
                        {
                            return;
                        }

                        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
                        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            return;
                        }

                        var accessToken = authHeader["Bearer ".Length..].Trim();
                        var jwt = context.SecurityToken as JwtSecurityToken;
                        var stillValidByClock = jwt is not null && jwt.ValidTo > DateTime.UtcNow;

                        var introspection = context.HttpContext.RequestServices
                            .GetRequiredService<KeycloakTokenIntrospectionService>();
                        var active = await introspection.IsTokenActiveAsync(
                            accessToken,
                            context.HttpContext.RequestAborted);

                        if (active)
                        {
                            return;
                        }

                        if (stillValidByClock)
                        {
                            context.HttpContext.Items[SessionTerminatedContext.HttpContextItemKey] = true;
                            context.Fail(SessionTerminatedContext.Message);
                            return;
                        }

                        context.Fail("Access token is not active.");
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (AuthorizationFailureContext.Get(context.HttpContext) is not null)
                        {
                            return Task.CompletedTask;
                        }

                        AuthorizationFailureContext.Set(
                            context.HttpContext,
                            new AuthorizationFailureSnapshot(
                                context.Exception?.Message ?? "JWT authentication failed.",
                                KeycloakClaims.GetKeycloakUserId(context.Principal)));
                        return Task.CompletedTask;
                    },
                    OnChallenge = async context =>
                    {
                        if (SessionTerminatedContext.IsTerminated(context.HttpContext))
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(
                                JsonSerializer.Serialize(new
                                {
                                    code = SessionTerminatedContext.ErrorCode,
                                    message = SessionTerminatedContext.Message
                                }),
                                context.HttpContext.RequestAborted);
                            return;
                        }

                        if (AuthorizationFailureContext.Get(context.HttpContext) is not null)
                        {
                            return;
                        }

                        AuthorizationFailureContext.Set(
                            context.HttpContext,
                            new AuthorizationFailureSnapshot("Authentication is required."));
                    },
                    OnForbidden = context =>
                    {
                        if (AuthorizationFailureContext.Get(context.HttpContext) is not null)
                        {
                            return Task.CompletedTask;
                        }

                        AuthorizationFailureContext.Set(
                            context.HttpContext,
                            new AuthorizationFailureSnapshot(
                                "Access token is valid but the user is not allowed to access this resource.",
                                KeycloakClaims.GetKeycloakUserId(context.HttpContext.User)));
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
        services.AddScoped<IAuthorizationHandler, AmmsPermissionAuthorizationHandler>();
        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

        return services;
    }

    public static IServiceCollection AddAmmsCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Spa", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    return;
                }

                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
