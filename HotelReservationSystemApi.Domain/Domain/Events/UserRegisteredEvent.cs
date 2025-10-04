namespace HotelReservationSystemAPI.Domain.Events
{
    public record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt) : INotification;

}
