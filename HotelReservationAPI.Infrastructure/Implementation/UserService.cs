using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public UserService(
            UserManager<User> userManager,
            IEventStore eventStore,
            IMediator mediator,
            IDistributedCache cache,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _eventStore = eventStore;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        // Non-registration example: Get events (for auditing)
        public async Task<APIResponse<IEnumerable<UserRegisteredEvent>>> GetUserEventsAsync(Guid userId)
        {
            var events = await _eventStore.GetEventsAsync<UserRegisteredEvent>(userId);
            if (!events.Any())
                return APIResponse<IEnumerable<UserRegisteredEvent>>.Fail(HttpStatusCode.NotFound, "No events found.");

            return APIResponse<IEnumerable<UserRegisteredEvent>>.Success(events, "Events retrieved successfully.");
        }
    }
}
