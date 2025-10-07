
using HotelReservationAPI.Domain.Interface;
using HotelReservationAPI.Infrastructure.Implementation;
using Humanizer;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System.Threading.RateLimiting;
using UserIdentity.Infrastructure.Persistence;


namespace HotelReservationSystemAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // DataBase AUTHDb

            builder.Services.AddDbContext<UserIdentityDB>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("HotelReservationAuthDb")));

            //Redis
            builder.Services.AddStackExchangeRedisCache(options =>
            options.Configuration = builder.Configuration.GetConnectionString("Redis"));

            // App Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEventBus, InMemoryEventBus>();

            // CQRS
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

            // services 
            builder.Services.AddScoped<IUserRepository,UserService>();
            builder.Services.AddScoped<IEventStore,EventStore>();
            builder.Services.AddScoped<IRoleRepository, RoleService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Rate Limiting(5 attempts / min for login)
                builder.Services.AddRateLimiter(options =>
                {
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.User?.GetUserId() ?? context.Request.Headers.Host.ToString(), // User or IP
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = 5,
                                Window = TimeSpan.FromMinutes(1)
                            }));
                });

            // Event Bus Subscribe (e.g., for Notification trigger)
            InMemoryEventBus.Subscribe(@event =>
            {
                // Log or publish to downstream
                Log.Information("Event published: {@Event}", @event);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
