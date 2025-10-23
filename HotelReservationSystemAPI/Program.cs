using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Api.Extensions;  // For API-specific extensions
using HotelReservationSystemAPI.Application.Extensions;  // For Application extensions>
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Extensions;
using HotelReservationSystemAPI.Infrastructure.Extensions;  // For Infrastructure extensions
using Microsoft.OpenApi.Models;
using Serilog;

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
// Core Services
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
// Layer-Specific Extensions (Modular Configuration)
// ------------------------------------------------------------
builder.Services.AddApplicationServices(builder.Configuration);  // CQRS, MediatR, Validators
builder.Services.AddInfrastructureServices(builder.Configuration);  // DB, Redis, Identity
builder.Services.AddApiServices(builder.Configuration);  // Auth, Rate Limiting, Swagger

var app = builder.Build();

// ------------------------------------------------------------
// Event Bus Subscription (Scoped for Best Practice)
// ------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
    eventBus.Subscribe<IDomainEvent>(async (IDomainEvent @event) =>
    {
        Log.Information("Event published: {@Event}", @event);
        await Task.CompletedTask;
    });
}

// ------------------------------------------------------------
// Configure Middleware Pipeline
// ------------------------------------------------------------
app.ConfigurePipeline(app.Environment);

app.Run();