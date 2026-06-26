using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace AMMS.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAmmsAuthentication(this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<AmmsAuthenticationOptions>(
            configuration.GetSection(AmmsAuthenticationOptions.SectionName));

        var authOptions = configuration
            .GetSection(AmmsAuthenticationOptions.SectionName)
            .Get<AmmsAuthenticationOptions>() ?? new AmmsAuthenticationOptions();

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
                    OnTokenValidated = context =>
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
                        }

                        return Task.CompletedTask;
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
                    OnChallenge = context =>
                    {
                        if (AuthorizationFailureContext.Get(context.HttpContext) is not null)
                        {
                            return Task.CompletedTask;
                        }

                        AuthorizationFailureContext.Set(
                            context.HttpContext,
                            new AuthorizationFailureSnapshot("Authentication is required."));
                        return Task.CompletedTask;
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

    public static IServiceCollection AddAmmsCors(this IServiceCollection services,IConfiguration configuration)
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
