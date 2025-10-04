namespace HotelReservationSystemAPI.Domain.Events
{
    public record RoleCreatedEvent(Guid RoleId, string Name, DateTime CreatedAt) : INotification;

}
