using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace HotelReservationSystemAPI.Application.CommandHandlers;



public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, APIResponse<RoleDto>>
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<CreateRoleHandler> _logger;
    private readonly IEventStore _eventStore;
    private readonly IEventBus _eventBus;
    private readonly IMediator _mediator;
    private readonly IDistributedCache _cache;

    public CreateRoleHandler(
        RoleManager<Role> roleManager,
        ILogger<CreateRoleHandler> logger,
        IEventStore eventStore,
        IEventBus eventBus,
        IMediator mediator,
        IDistributedCache cache)
    {
        _roleManager = roleManager;
        _logger = logger;
        _eventStore = eventStore;
        _eventBus = eventBus;
        _mediator = mediator;
        _cache = cache;
    }

    public async Task<APIResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to create role {RoleName}", request.Name);

        // Check cache
        var cached = await _cache.GetStringAsync($"role:{request.Name}");
        if (cached != null)
        {
            var cachedRole = JsonSerializer.Deserialize<Role>(cached);
            return APIResponse<RoleDto>.Success(RoleDto.FromRole(cachedRole!), "Role retrieved from cache");
        }

        // Check existing role
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
            return APIResponse<RoleDto>.Fail(HttpStatusCode.Conflict, $"Role '{request.Name}' already exists");

        //  Domain validation and creation
        var creationResult = Role.Create(request.Name);
        if (!creationResult.IsSuccess)
            return APIResponse<RoleDto>.Fail(HttpStatusCode.BadRequest, creationResult.Error ?? "Invalid role");

        var roleData = creationResult.Value;
        if (roleData?.Role == null)
            return APIResponse<RoleDto>.Fail(HttpStatusCode.InternalServerError, "Failed to create role data");

        var role = roleData.Role;
        var domainEvent = roleData.DomainEvent;

        //  Persist in Identity
        var identityResult = await _roleManager.CreateAsync(role);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            return APIResponse<RoleDto>.Fail(HttpStatusCode.BadRequest, errors);
        }

        //  Cache
        await _cache.SetStringAsync($"role:{request.Name}", JsonSerializer.Serialize(role),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) });

        //  Publish events
        await _eventStore.SaveEventAsync(domainEvent);
        await _mediator.Publish(domainEvent, cancellationToken);
        _eventBus.Publish(domainEvent);

        _logger.LogInformation("Role {RoleName} created successfully", request.Name);
        return APIResponse<RoleDto>.Success(RoleDto.FromRole(role), "Role created successfully");
    }
}
