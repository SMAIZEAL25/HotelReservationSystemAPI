namespace HotelReservationAPI.Application.Interface
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event);
        void Subscribe(Func<object, Task> handler);
    }
}
