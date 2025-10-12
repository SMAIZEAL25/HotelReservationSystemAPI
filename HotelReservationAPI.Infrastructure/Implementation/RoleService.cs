
using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
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
            // Check cache first
            var cached = await _cache.GetStringAsync($"role:{roleName}");
            if (cached != null)
            {
                var cachedRole = JsonSerializer.Deserialize<Role>(cached);
                return APIResponse<Role>.Success(cachedRole!, "Role retrieved from cache");
            }

            var existingRole = await _roleManager.FindByNameAsync(roleName);
            if (existingRole != null)
                return APIResponse<Role>.Fail(HttpStatusCode.Conflict, $"Role '{roleName}' already exists");

            var role = new Role { Name = roleName };
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Role creation failed: {errors}");
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, errors);
            }

            await _cache.SetStringAsync($"role:{roleName}", JsonSerializer.Serialize(role),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
                });

            _logger.LogInformation($"Role '{roleName}' created successfully");

            return APIResponse<Role>.Success(role, "Role created successfully");
        }
    }
}
