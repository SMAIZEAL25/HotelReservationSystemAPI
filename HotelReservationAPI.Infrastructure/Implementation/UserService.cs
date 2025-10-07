using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Implementation
{
    public class UserService : IUserRepository
    {
                    
            private readonly UserManager<User> _userManager;
            private readonly IPasswordHasher<User> _passwordHasher;
            private readonly IEventStore _eventStore;
            private readonly IMediator _mediator;
            private readonly IDistributedCache _cache;
            private readonly ILogger<UserService> _logger;

            public UserService(
                UserManager<User> userManager,
                IPasswordHasher<User> passwordHasher,
                IEventStore eventStore,
                IMediator mediator,
                IDistributedCache cache,
                ILogger<UserService> logger)
            {
                _userManager = userManager;
                _passwordHasher = passwordHasher;
                _eventStore = eventStore;
                _mediator = mediator;
                _cache = cache;
                _logger = logger;
            }

            public async Task<APIResponse<User>> RegisterUserAsync(UserRegisterDto dto)
            {
                // Hash function that will be passed to domain
                Func<string, string> hashFunction = (plainPassword) =>
                {
                    var tempUser = new User();
                    return _passwordHasher.HashPassword(tempUser, plainPassword);
                };

                // Call domain factory method
                var domainResult = User.Create(
                    dto.FullName,
                    dto.Email,
                    dto.Password,
                    dto.Role,
                    hashFunction
                );

                if (!domainResult.IsSuccess)
                {
                    _logger.LogError($"User creation validation failed: {domainResult.Message}");
                    return APIResponse<User>.Fail(HttpStatusCode.BadRequest, domainResult.Message);
                }

                var user = domainResult.User!;

                // Save user in identity
                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError($"User creation failed for {dto.Email}: {errors}");
                    return APIResponse<User>.Fail(HttpStatusCode.BadRequest, $"User creation failed: {errors}");
                }

                // Add role
                var addRoleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError($"Role assignment failed for {dto.Email}");
                    await _userManager.DeleteAsync(user);
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    return APIResponse<User>.Fail(HttpStatusCode.BadRequest, $"Role assignment failed: {errors}");
                }

                // Publish domain event
                if (domainResult.DomainEvent != null)
                {
                    await _eventStore.SaveEventAsync(domainResult.DomainEvent);
                    await _mediator.Publish(domainResult.DomainEvent);
                }

                // Cache role (optional optimization)
                await _cache.SetStringAsync(
                    $"role:{dto.Role}",
                    JsonSerializer.Serialize(user.RoleValueObject),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) }
                );

                _logger.LogInformation($"User {dto.Email} registered successfully at {DateTime.UtcNow}");

                return APIResponse<User>.Success(user, "User created successfully");
            }
        }
    }

