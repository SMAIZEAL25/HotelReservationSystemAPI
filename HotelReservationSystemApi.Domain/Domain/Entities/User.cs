using HotelReservationAPI.Infrastructure.Repositories.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Domain.ValueObject;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;

namespace HotelReservationSystemAPI.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = string.Empty;
        public Email EmailValueObject { get; private set; } = default!;
        public Password PasswordValueObject { get; private set; } = default!;
        public RoleValue RoleValueObject { get; private set; } = default!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private User() { } // EF Core

        public static async Task<APIResponse<User>> CreateAsync(
            UserRegisterDto dto,
            UserManager<User> userManager,
            IPasswordHasher<User> passwordHasher,
            IEventStore eventStore,
            IMediator mediator,
            IDistributedCache cache,
            ILoggerService logger)
        {
            // Validate email
            var emailResult = Email.Create(dto.Email);
            if (!emailResult.IsSuccess)
                return APIResponse<User>.Fail(HttpStatusCode.BadRequest, emailResult.Message);

            // Validate password
            var passwordResult = Password.Create(dto.Password);
            if (!passwordResult.IsSuccess)
                return APIResponse<User>.Fail(HttpStatusCode.BadRequest, passwordResult.Message);

            // Validate role
            var roleResult = RoleValue.Create(dto.Role);
            if (!roleResult.IsSuccess)
                return APIResponse<User>.Fail(HttpStatusCode.BadRequest, roleResult.Message);

            // Create user instance
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                UserName = dto.Email,
                Email = dto.Email,
                EmailValueObject = emailResult.Data!,
                PasswordValueObject = passwordResult.Data!,
                RoleValueObject = roleResult.Data!,
                CreatedAt = DateTime.UtcNow
            };

            // Save user in identity
            var createResult = await userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError($"User creation failed for {dto.Email}: {errors}");
                return APIResponse<User>.Fail(HttpStatusCode.BadRequest, $"User creation failed: {errors}");
            }

            // Add role
            var addRoleResult = await userManager.AddToRoleAsync(user, dto.Role);
            if (!addRoleResult.Succeeded)
            {
                logger.LogError($"Role assignment failed for {dto.Email}");
                await userManager.DeleteAsync(user);
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                return APIResponse<User>.Fail(HttpStatusCode.BadRequest, $"Role assignment failed: {errors}");
            }

            // Save registration event
            var userEvent = new UserRegisteredEvent(user.Id, dto.Email, DateTime.UtcNow);
            await eventStore.SaveEventAsync(userEvent);
            await mediator.Publish(userEvent);

            // Cache role (optional optimization)
            await cache.SetStringAsync($"role:{dto.Role}",
                JsonSerializer.Serialize(roleResult.Data),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) });

            logger.LogInformation($"User {dto.Email} registered successfully at {DateTime.UtcNow}");

            return APIResponse<User>.Success(user, "User created successfully");
        }
    }
}
