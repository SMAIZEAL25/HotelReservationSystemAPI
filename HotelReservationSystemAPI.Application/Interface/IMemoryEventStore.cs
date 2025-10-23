
using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Interface
{
    public class InMemoryEventStore : IEventStore
    {
        private static readonly ConcurrentDictionary<Guid, List<IDomainEvent>> _store = new();

        public Task SaveEventAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            var aggregateId = @event.AggregateId;  
            if (!_store.ContainsKey(aggregateId))
                _store[aggregateId] = new List<IDomainEvent>();

            _store[aggregateId].Add(@event);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>(Guid aggregateId) where TEvent : IDomainEvent
        {
            if (_store.TryGetValue(aggregateId, out var events))
            {
                return Task.FromResult(events.OfType<TEvent>());
            }
            return Task.FromResult(Enumerable.Empty<TEvent>());
        }
    }
}