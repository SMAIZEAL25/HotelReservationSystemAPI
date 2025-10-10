using HotelReservationAPI.Domain.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Interface
{

    //You can later replace this with RabbitMQ, Kafka,
    //or Azure Service Bus without changing your app logic — thanks to the IEventBus abstraction.
    public class InMemoryEventBus : IEventBus
    {
        private static readonly ConcurrentBag<Func<object, Task>> _subscribers = new();

        public void Publish<TEvent>(TEvent @event)
        {
            foreach (var subscriber in _subscribers)
            {
                _ = subscriber.Invoke(@event);
            }
        }

        public void Subscribe(Func<object, Task> handler)
        {
            _subscribers.Add(handler);
        }
    }
}
