using HotelReservationSystemAPI.Domain.Events;

namespace HotelReservationSystemAPI.Domain.Entities
{


    public class RoleCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid RoleId { get; }
        public string Name { get; }

        public RoleCreatedEvent(Guid roleId, string name)
        {
            RoleId = roleId;
            Name = name;
        }
    }
}



