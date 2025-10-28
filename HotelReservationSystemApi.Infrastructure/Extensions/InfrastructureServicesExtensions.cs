
using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Implementation;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Infrastructure.Implementation;
using HotelReservationSystemAPI.Infrastructure.MiddleWare;
using HotelReservationSystemAPI.Infrastructure.Persistence;
using HotelReservationSystemAPI.Infrastructure.RedisCacheServie;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;  
using Microsoft.EntityFrameworkCore;  
using Microsoft.EntityFrameworkCore.SqlServer;  
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace HotelReservationSystemAPI.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // Database Context
        services.AddDbContext<UserIdentityDB>(options =>
            options.UseSqlServer(config.GetConnectionString("HotelReservationAuthDb")));

        // Redis Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = config.GetConnectionString("Redis");
        });
        services.AddScoped<RedisCacheService>();

        // Identity
        services.AddIdentity<User, Role>(options =>
        {
            // Password requirements
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<UserIdentityDB>()
        .AddDefaultTokenProviders();

        // Repositories & Services (concretions for Application interfaces)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<IEventStore, InMemoryEventStore>();  
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRateLimiter,RedisTokenBucketRateLimiter>();

        // Rate Limiting (Fixed: Full options)
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var userId = context.User?.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId")?.Value;
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                var partitionKey = !string.IsNullOrEmpty(userId) ? $"user:{userId}" : $"ip:{ipAddress}";

                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1)
                });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
            };
        });

        return services;  
    }
}