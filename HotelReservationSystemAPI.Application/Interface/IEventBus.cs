using HotelReservationSystemAPI.Domain.Events;

namespace HotelReservationAPI.Application.Interface
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event) where TEvent : IDomainEvent;
        void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
    }
}
