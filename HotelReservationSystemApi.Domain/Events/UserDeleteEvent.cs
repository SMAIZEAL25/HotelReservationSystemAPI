using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class UserDeletedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid AggregateId { get; }
        public string Email { get; }

        public UserDeletedEvent(Guid userId, string email)
        {
            AggregateId = userId;
            Email = email;
        }
    }
}
