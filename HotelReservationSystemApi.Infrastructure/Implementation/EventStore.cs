using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;


namespace HotelReservationSystemAPI.Infrastructure.Implementation;

public class EventStore : IEventStore
{
    private readonly UserIdentityDB _context;
    private readonly ILogger<EventStore> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EventStore(UserIdentityDB context, ILogger<EventStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveEventAsync<T>(T @event) where T : IDomainEvent
    {
        try
        {
            var eventData = JsonSerializer.Serialize(@event, _jsonOptions);
            var dbEvent = new DomainEvent
            {
                Id = Guid.NewGuid(),
                AggregateId = @event.EventId,  // Use interface prop
                EventType = typeof(T).Name,
                Data = eventData,
                OccurredAt = @event.OccurredAt  // Use interface prop
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

    public async Task<IEnumerable<T>> GetEventsAsync<T>(Guid aggregateId) where T : IDomainEvent
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
                return Enumerable.Empty<T>();
            }

            var events = storedEvents.Select(e =>
                JsonSerializer.Deserialize<T>(e.Data, _jsonOptions) ?? throw new InvalidOperationException("Event deserialization failed"))
                .Where(e => e != null)!;  // Filter nulls

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for AggregateId {AggregateId}", aggregateId);
            throw;
        }
    }
}
