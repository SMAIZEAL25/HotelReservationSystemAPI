using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
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

        // 1. Check cache first
        var cached = await _cache.GetStringAsync($"role:{request.Name}");
        if (cached != null)
        {
            var cachedRole = JsonSerializer.Deserialize<Role>(cached);
            _logger.LogInformation("Role {RoleName} retrieved from cache", request.Name);
            return APIResponse<RoleDto>.Success(RoleDto.FromRole(cachedRole!), "Role retrieved from cache");
        }

        // 2. Check existence (Idempotency)
        var existingRole = await _roleManager.FindByNameAsync(request.Name);
        if (existingRole != null)
        {
            _logger.LogWarning("Role {RoleName} already exists", request.Name);
            return APIResponse<RoleDto>.Fail(HttpStatusCode.Conflict, $"Role '{request.Name}' already exists");
        }

        // 3. Create via domain factory
        var creationResult = Role.Create(request.Name);
        if (!creationResult.IsSuccess)
        {
            _logger.LogError("Domain validation failed for role {RoleName}: {Error}", request.Name, creationResult.Error);
            return APIResponse<RoleDto>.Fail(HttpStatusCode.BadRequest, creationResult.Error);
        }

        var roleCreationData = creationResult.Value;
        if (roleCreationData.Role == null)
        {
            _logger.LogError("Domain factory returned null Role for {RoleName}", request.Name);
            return APIResponse<RoleDto>.Fail(HttpStatusCode.InternalServerError, "Failed to create role data.");
        }

        var role = roleCreationData.Role;
        var domainEvent = roleCreationData.DomainEvent;  // Assumes RoleCreatedEvent from factory

        // 4. Persist with Identity
        var identityResult = await _roleManager.CreateAsync(role);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            _logger.LogError("Identity role creation failed for {RoleName}: {Errors}", request.Name, errors);
            return APIResponse<RoleDto>.Fail(HttpStatusCode.BadRequest, errors);
        }

        // 5. Cache
        await _cache.SetStringAsync($"role:{request.Name}", JsonSerializer.Serialize(role),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) });

        // 6. Events
        await _eventStore.SaveEventAsync(domainEvent);
        await _mediator.Publish(domainEvent, cancellationToken);
        _eventBus.Publish(domainEvent);

        _logger.LogInformation("Role {RoleName} created successfully", request.Name);

        return APIResponse<RoleDto>.Success(RoleDto.FromRole(role), "Role created successfully");
    }
}