using HotelReservationAPI.Infrastructure.Repositories.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Domain.ValueObject;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;

namespace HotelReservationSystemAPI.Domain.Entities
{
    public class Role : IdentityRole<Guid>
    {
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        private Role() { }

        public static async Task<APIResponse<Role>> CreateAsync(
            string roleName,
            RoleManager<Role> roleManager,
            IEventStore eventStore,
            IMediator mediator,
            IDistributedCache cache,
            ILoggerService logger)
        {
            var roleValidation = RoleValue.Create(roleName);
            if (!roleValidation.IsSuccess)
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, roleValidation.Message);

            var exists = await roleManager.FindByNameAsync(roleName);
            if (exists != null)
                return APIResponse<Role>.Fail(HttpStatusCode.Conflict, $"Role '{roleName}' already exists");

            var role = new Role { Id = Guid.NewGuid(), Name = roleName };

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError($"Role creation failed for {roleName}: {errors}");
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, $"Failed to create role: {errors}");
            }

            // Cache role
            await cache.SetStringAsync($"role:{roleName}", JsonSerializer.Serialize(roleValidation.Data),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

            // Publish event
            var roleEvent = new RoleCreatedEvent(role.Id, roleName, DateTime.UtcNow);
            await eventStore.SaveEventAsync(roleEvent);
            await mediator.Publish(roleEvent);

            logger.LogInformation($"Role {roleName} created successfully");
            return APIResponse<Role>.Success(role, "Role created successfully");
        }
    }
}
