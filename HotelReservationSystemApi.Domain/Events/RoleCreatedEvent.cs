using HotelReservationSystemAPI.Domain.Events;

namespace HotelReservationSystemAPI.Domain.Entities
{


    public class RoleCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid AggregateId { get; }  // RoleId as aggregate ID for event sourcing
        public string Name { get; }

        public RoleCreatedEvent(Guid roleId, string name)
        {
            AggregateId = roleId;  // Set from ctor for correlation
            Name = name ?? throw new ArgumentNullException(nameof(name));  // Null-safe
        }
    }
}



