using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Implementation
{
    public class RoleService 
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IEventStore _eventStore;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            RoleManager<Role> roleManager,
            IEventStore eventStore,
            IMediator mediator,
            IDistributedCache cache,
            ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _eventStore = eventStore;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<APIResponse<Role>> CreateRoleAsync(string roleName)
        {
            // Check if role already exists
            var exists = await _roleManager.FindByNameAsync(roleName);
            if (exists != null)
                return APIResponse<Role>.Fail(HttpStatusCode.Conflict, $"Role '{roleName}' already exists");

            // Call domain factory method
            var domainResult = Role.Create(roleName);

            if (!domainResult.IsSuccess)
            {
                _logger.LogError($"Role creation validation failed: {domainResult.Message}");
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, domainResult.Message);
            }

            var role = domainResult.Role!;

            // Save role in identity
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Role creation failed for {roleName}: {errors}");
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, $"Failed to create role: {errors}");
            }

            // Cache role
            if (domainResult.RoleValue != null)
            {
                await _cache.SetStringAsync(
                    $"role:{roleName}",
                    JsonSerializer.Serialize(domainResult.RoleValue),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }
                );
            }

            // Publish domain event
            if (domainResult.DomainEvent != null)
            {
                await _eventStore.SaveEventAsync(domainResult.DomainEvent);
                await _mediator.Publish(domainResult.DomainEvent);
            }

            _logger.LogInformation($"Role {roleName} created successfully");

            return APIResponse<Role>.Success(role, "Role created successfully");
        }

        public Task<APIResponse> GetByNameAsync()
        {
            throw new NotImplementedException();
        }
    }
}

// Policies for authentication and authorization  as a guest manager and admin 

