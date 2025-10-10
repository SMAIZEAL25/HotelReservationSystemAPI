using HotelReservationSystemAPI.Application.Events;

namespace HotelReservationSystemAPI.Application.Entities
{


    public record RoleCreatedEvent(Guid RoleId, string RoleName) : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}


