using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
                            context.Fail("Token authorized party is not allowed.");
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

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
