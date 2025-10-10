using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Implementation
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IEventStore _eventStore;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailService _emailService;

        public UserService(
            UserManager<User> userManager,
            IEventStore eventStore,
            IMediator mediator,
            IDistributedCache cache,
            ILogger<UserService> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _eventStore = eventStore;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<APIResponse<UserDto>> RegisterUserAsync(UserDto dto)
        {
            _logger.LogInformation("Attempting to register new user with email {Email}", dto.Email);

            // Idempotency: Ensure the email is not already taken
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("User registration failed: email {Email} already exists", dto.Email);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, "Email is already registered.");
            }

            // Create user domain entity (unconfirmed by default)
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                UserName = dto.Email,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false  // Part of registration flow: Start unconfirmed
            };

            // Create user using ASP.NET Identity (handles hashing automatically)
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("User creation failed for {Email}: {Errors}", dto.Email, errors);
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"User creation failed: {errors}");
            }

            // Assign role (ensure roles are already seeded)
            var roleName = dto.Role.ToString();
            var addRoleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Role assignment failed for {Email}", dto.Email);
                await _userManager.DeleteAsync(user); // Rollback
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                return APIResponse<UserDto>.Fail(HttpStatusCode.BadRequest, $"Role assignment failed: {errors}");
            }

            // ✅ Part 1 of Confirmation Flow: Generate & store token, send email
            var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await _userManager.SetEmailConfirmationTokenAsync(user, confirmationToken); // Securely store in Identity
            var confirmationLink = $"https://yourapi.com/api/users/confirm-email?email={user.Email}&token={confirmationToken}";
            await _emailService.SendConfirmationEmailAsync(user.Email, user.FullName, confirmationLink);

            // Raise domain event for registration
            var domainEvent = new UserRegisteredEvent(user.Id, user.Email);
            await _eventStore.SaveEventAsync(domainEvent);
            await _mediator.Publish(domainEvent);

            // Cache user details in Redis (email and name only, exclude password) - TTL 24 hours
            var cacheKey = $"user:{user.Id}";
            var userCacheData = new { Email = user.Email, FullName = user.FullName };
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(userCacheData),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });

            // Cache role for optimization
            await _cache.SetStringAsync(
                $"role:{dto.Email}",
                JsonSerializer.Serialize(user.Role),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) });

            _logger.LogInformation("User {Email} registered successfully with role {Role}; confirmation email sent as part of registration flow", dto.Email, dto.Role);

            var userDto = UserDto.FromUser(user); // Exclude sensitive data
            return APIResponse<UserDto>.Success(userDto, "User registered successfully. Please check your email to confirm your account.");
        }
    }
}

