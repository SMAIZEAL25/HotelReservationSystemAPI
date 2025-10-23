
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

    // This is an application-level pub/sub system —
    // it lets different parts of your app react to events without coupling them together.
    // Later, you can swap it out for RabbitMQ, Kafka, or Azure Service Bus by writing a different
    // implementation of IEventBus.
    public class InMemoryEventBus : IEventBus
    {
        private static readonly ConcurrentBag<Func<IDomainEvent, Task>> _subscribers = new();

        public void Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            foreach (var subscriber in _subscribers)
            {
                _ = subscriber(@event);  // Fire-and-forget
            }
        }

        public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
        {
            _subscribers.Add(async (e) => await handler((TEvent)e));  // Cast to specific type
        }
    }
}
