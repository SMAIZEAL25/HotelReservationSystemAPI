using HotelReservationSystemAPI.Domain.Events;

namespace HotelReservationSystemAPI.Domain.Entities
{


    public record RoleCreatedEvent(Guid RoleId, string RoleName) : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    }
}


