namespace HotelReservationAPI.Domain.Interface
{
    public interface IEventStore
    {
        Task SaveEventAsync<TEvent>(TEvent @event) where TEvent : class;
        Task<IEnumerable<TEvent>> GetEventsAsync<TEvent>(Guid aggregateId) where TEvent : class;
    }
}
