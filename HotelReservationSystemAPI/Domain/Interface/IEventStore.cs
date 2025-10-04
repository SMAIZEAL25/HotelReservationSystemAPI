namespace HotelReservationSystemAPI.Domain.Interface
{
    public interface IEventStore
    {
        Task SaveEventAsync<T>(T @event) where T : class;

        Task<IEnumerable<T>> GetEventsAsync<T>(Guid aggregateId);
    }
}
