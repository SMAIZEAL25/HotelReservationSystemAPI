
using HotelReservationAPI.Domain.Interface;
using HotelReservationAPI.Infrastructure.Implementation;
using HotelReservationAPI.Infrastructure.MiddleWare;
using HotelReservationAPI.Infrastructure.TokenProvider;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.Events;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Services;
using HotelReservationSystemAPI.Domain.Entities;
using Humanizer;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using UserIdentity.Infrastructure.Persistence;


namespace HotelReservationSystemAPI
{
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
                .CreateLogger();

            builder.Host.UseSerilog();

            // ------------------------------------------------------------
            // Add services to the container
            // ------------------------------------------------------------
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            builder.Services.AddScoped<RedisCacheService>();
            builder.Services.AddSingleton<RedisTokenBucketRateLimiter>();


            // ------------------------------------------------------------
            // Register Application Services
            // ------------------------------------------------------------
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

            // ------------------------------------------------------------
            // CQRS - MediatR Registration
            // ------------------------------------------------------------
            builder.Services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(
                    Assembly.GetExecutingAssembly(),
                    typeof(RegisterUserCommand).Assembly,
                    typeof(UserRegisteredEvent).Assembly
                ));

            // ------------------------------------------------------------
            // Repository and Unit of Work
            // ------------------------------------------------------------
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IEventStore, EventStore>(); // needs to be looked into 
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
            builder.Services.AddSingleton<IEventStore, InMemoryEventStore>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<RedisTokenBucketRateLimiter>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RoleService>();

            // ------------------------------------------------------------
            // Rate Limiting (5 attempts / min for login)
            // ------------------------------------------------------------
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var userId = context.User?.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId")?.Value;
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

                    // Combine both for fair tracking
                    var partitionKey = !string.IsNullOrEmpty(userId)
                        ? $"user:{userId}"
                        : $"ip:{ipAddress}";

                    // Use RedisTokenBucketRateLimiter to determine allowance
                    return RateLimitPartition.GetConcurrencyLimiter(partitionKey, _ =>
                    {
                        return new ConcurrencyLimiterOptions
                        {
                            PermitLimit = 1,
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        };
                    });
                });

                // Custom rejection handler
                options.OnRejected = async (context, token) =>
                {
                    var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                    var limiter = new RedisTokenBucketRateLimiter(cache, 50, 25, TimeSpan.FromSeconds(1));

                    var userId = context.HttpContext.User?.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId")?.Value;
                    var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
                    var partitionKey = !string.IsNullOrEmpty(userId) ? $"user:{userId}" : $"ip:{ipAddress}";

                    var allowed = await limiter.AllowRequestAsync(partitionKey);
                    if (!allowed)
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            error = "Too many requests. Please slow down.",
                            key = partitionKey,
                            timestamp = DateTime.UtcNow
                        });
                    }
                };
            });

            // Authentication and Authorization

            builder.Services.AddIdentity<User, Role>()
                 .AddEntityFrameworkStores<UserIdentityDB>()
                    .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", options =>
            {
                var config = builder.Configuration;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
                options.AddPolicy("RequireGuestRole", policy => policy.RequireRole("Guest"));
            });

            // ------------------------------------------------------------
            // Build Application
            // ------------------------------------------------------------
            var app = builder.Build();

            // ------------------------------------------------------------
            // Resolve Event Bus and Subscribe to Global Event Logging
            // ------------------------------------------------------------
            var eventBus = app.Services.GetRequiredService<IEventBus>();

            eventBus.Subscribe(async @event =>
            {
                Log.Information("Event published: {@Event}", @event);
                await Task.CompletedTask;
            });

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
}
