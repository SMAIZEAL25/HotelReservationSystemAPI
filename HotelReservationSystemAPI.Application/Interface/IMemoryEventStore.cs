using HotelReservationAPI.Domain.Interface;
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
        private static readonly ConcurrentDictionary<Guid, List<object>> _store = new();

        public Task SaveEventAsync<TEvent>(TEvent @event) where TEvent : class
        {
            var aggregateId = (Guid)@event.GetType().GetProperty("UserId")!.GetValue(@event)!;
            if (!_store.ContainsKey(aggregateId))
                _store[aggregateId] = new List<object>();

            _store[aggregateId].Add(@event);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>(Guid aggregateId) where TEvent : class
        {
            if (_store.TryGetValue(aggregateId, out var events))
            {
                return Task.FromResult(events.OfType<TEvent>());
            }
            return Task.FromResult(Enumerable.Empty<TEvent>());
        }
    }
}
