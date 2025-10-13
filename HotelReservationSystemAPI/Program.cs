using FluentValidation;
using HotelReservationAPI.Application.Interface;
using HotelReservationAPI.Application.MiddleWare;
using HotelReservationAPI.Infrastructure.Implementation;
using HotelReservationAPI.Infrastructure.TokenProvider;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Services;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Domain.Interface;
using HotelReservationSystemAPI.Infrastructure.Implementation;
using HotelReservationSystemAPI.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using UserIdentity.Infrastructure.Persistence;

namespace HotelReservationSystemAPI.Api;  // Standardized namespace for API layer

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ------------------------------------------------------------
        // Serilog Configuration
        // ------------------------------------------------------------
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();

        // ------------------------------------------------------------
        // Add services to the container
        // ------------------------------------------------------------
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotel Reservation System API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
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

        // ------------------------------------------------------------
        // Database Context
        // ------------------------------------------------------------
        builder.Services.AddDbContext<UserIdentityDB>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("HotelReservationAuthDb")));

        // ------------------------------------------------------------
        // Redis Cache Configuration
        // ------------------------------------------------------------
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetConnectionString("Redis");
        });

        // ------------------------------------------------------------
        // Identity Configuration
        // ------------------------------------------------------------
        builder.Services.AddIdentity<User, Role>(options =>
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

        // ------------------------------------------------------------
        // JWT Authentication
        // ------------------------------------------------------------
        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
        var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is missing");
        var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is missing");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        // ------------------------------------------------------------
        // Authorization Policies
        // ------------------------------------------------------------
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin", "HotelAdmin"));
            options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
        });

        // ------------------------------------------------------------
        // CQRS - MediatR Registration
        // ------------------------------------------------------------
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(Program).Assembly,
            typeof(RegisterUserCommand).Assembly,
            typeof(UserRegisteredEvent).Assembly
        ));

        // ------------------------------------------------------------
        // FluentValidation (if using validators)
        // ------------------------------------------------------------
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        // ------------------------------------------------------------
        // Repository and Services Registration (No Duplicates)
        // ------------------------------------------------------------
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEventStore, EventStore>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
        builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();  
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<RedisTokenBucketRateLimiter>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<RoleService>();
        builder.Services.AddScoped<RedisCacheService>();

        // ------------------------------------------------------------
        // Rate Limiting (Custom RedisTokenBucketRateLimiter)
        // ------------------------------------------------------------
        builder.Services.AddRateLimiter(options =>
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

        // ------------------------------------------------------------
        // Build Application
        // ------------------------------------------------------------
        var app = builder.Build();

        // ------------------------------------------------------------
        // Event Bus Subscription (Use Scoped Service for Better Practice)
        // ------------------------------------------------------------
        using (var scope = app.Services.CreateScope())
        {
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe(@event =>
            {
                Log.Information("Event published: {@Event}", @event);
            });
        }

        // ------------------------------------------------------------
        // Configure Middleware Pipeline
        // ------------------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        app.UseRateLimiter();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}