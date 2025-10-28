using HotelReservationSystemAPI.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;  // Explicit for options
using System.Text;

namespace HotelReservationSystemAPI.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration config)
    {
        // Chain feature-specific extensions for modularity
        return services
            .AddJwtAuthentication(config)
            .AddAuthorizationPolicies()
            .AddSwaggerDocumentation();  // Now idempotent
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = GetJwtSettings(config);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.FromMinutes(5)  // Grace period for clock drift (finance-friendly)
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Log failure (inject ILogger if needed)
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsync("{\"error\":\"Invalid token\"}");
                    }
                };
            });
        return services;
    }

    private static JwtSettings GetJwtSettings(IConfiguration config)
    {
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing from configuration.");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing from configuration.");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing from configuration.");
        return new JwtSettings { Key = key, Issuer = issuer, Audience = audience };
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin", "HotelAdmin"));
            options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
            // Extensible: Add more as needed, e.g., options.AddPolicy("RequireBankAuditor", ...);
        });
        return services;
    }

    private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        // Idempotent: Register SwaggerGen ONLY if not already added (prevents duplicates)
        var swaggerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ISwaggerProvider));
        if (swaggerDescriptor != null) return services;  // Already registered—skip

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Hotel Reservation System API",
                Version = "v1",
                Description = "Secure API for user auth, roles, and reservations."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer [token]'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        return services;
    }

    // Helper record for JWT config (C# 9+; keeps it simple)
    private record JwtSettings
    {
        public string Key { get; init; } = string.Empty;
        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
    }
}