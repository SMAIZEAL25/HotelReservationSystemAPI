using FluentValidation;
using HotelReservationSystemAPI.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace HotelReservationSystemAPI.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // ✅ MediatR CQRS setup (for v11)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(ApplicationServiceExtensions).Assembly
        ));

        // ✅ FluentValidation (auto-discover validators)
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        // ✅ AutoMapper (if using – add profiles)
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);

        // ✅ Application Services
        services.AddScoped<UserService>();
        services.AddScoped<RoleService>();
        services.AddScoped<AuthenticationService>();

        return services;
    }
}