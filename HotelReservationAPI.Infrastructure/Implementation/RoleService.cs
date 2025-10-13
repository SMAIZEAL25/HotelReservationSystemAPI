using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;

namespace HotelReservationSystemAPI.Infrastructure.Implementation;

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

    // Non-CQRS example: Get role events (for auditing)
    public async Task<APIResponse<IEnumerable<RoleCreatedEvent>>> GetRoleEventsAsync(string roleName)
    {
        var events = await _eventStore.GetEventsAsync<RoleCreatedEvent>(roleName); // Assume GetByAggregateId
        if (!events.Any())
            return APIResponse<IEnumerable<RoleCreatedEvent>>.Fail(HttpStatusCode.NotFound, "No events found.");

        return APIResponse<IEnumerable<RoleCreatedEvent>>.Success(events, "Events retrieved successfully.");
    }
}