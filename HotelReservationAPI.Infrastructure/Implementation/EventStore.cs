using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UserIdentity.Infrastructure.Persistence;

namespace HotelReservationAPI.Infrastructure.Implementation
{ 
        public class EventStore : IEventStore
        {
        private readonly UserIdentityDB _context;
        private readonly ILogger<EventStore> _logger;

        public EventStore(UserIdentityDB context, ILogger<EventStore> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SaveEventAsync<T>(T @event) where T : class
        {
            try
            {
                var eventData = JsonSerializer.Serialize(@event);
                var dbEvent = new DomainEvent
                {
                    Id = Guid.NewGuid(),
                    AggregateId = GetAggregateId(@event),
                    EventType = typeof(T).Name,
                    Data = eventData,
                    OccurredAt = DateTime.UtcNow
                };

                await _context.DomainEvents.AddAsync(dbEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Event {EventType} saved successfully for AggregateId {AggregateId}",
                    typeof(T).Name, dbEvent.AggregateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save event {EventType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetEventsAsync<T>(Guid aggregateId)
        {
            try
            {
                var storedEvents = await _context.DomainEvents
                    .Where(e => e.AggregateId == aggregateId && e.EventType == typeof(T).Name)
                    .OrderBy(e => e.OccurredAt)
                    .ToListAsync();

                if (!storedEvents.Any())
                {
                    _logger.LogWarning("No events found for AggregateId {AggregateId}", aggregateId);
                }

                return storedEvents.Select(e =>
                    JsonSerializer.Deserialize<T>(e.Data) ?? throw new InvalidOperationException("Event deserialization failed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for AggregateId {AggregateId}", aggregateId);
                throw;
            }
        }

        private static Guid GetAggregateId<T>(T @event)
        {
            // Convention: all domain events must expose AggregateId or Id property
            var prop = typeof(T).GetProperty("UserId") ?? typeof(T).GetProperty("AggregateId") ?? typeof(T).GetProperty("Id");
            if (prop == null)
                throw new InvalidOperationException($"Event type {typeof(T).Name} must contain an AggregateId, UserId, or Id property.");

            return (Guid)prop.GetValue(@event)!;
        }
    }
}

