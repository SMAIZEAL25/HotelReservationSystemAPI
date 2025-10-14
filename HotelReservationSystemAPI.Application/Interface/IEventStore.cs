using HotelReservationSystemAPI.Domain.Events;

namespace HotelReservationAPI.Application.Interface
{
    public interface IEventStore
    {
        Task SaveEventAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
        Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>(Guid aggregateId) where TEvent : IDomainEvent;
    }
}
