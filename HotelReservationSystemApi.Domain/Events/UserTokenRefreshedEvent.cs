using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class UserTokenRefreshedEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid AggregateId { get; }  // UserId
        public string Email { get; }

        public UserTokenRefreshedEvent(Guid userId, string email)
        {
            AggregateId = userId;
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }
    }
}
