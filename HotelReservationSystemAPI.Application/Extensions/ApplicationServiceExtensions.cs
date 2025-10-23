using FluentValidation;
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
        // MediatR CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
             typeof(ApplicationServiceExtensions).Assembly,
             typeof(Program).Assembly
         ));

        // FluentValidation (auto-discover validators)
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        // AutoMapper (if using – add profiles)
        services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);

        // Application Services (abstract or Application-only)
        services.AddScoped<UserService>();  
        services.AddScoped<RoleService>();  
        services.AddScoped<AuthenticationService>();  

        return services;
    }
}