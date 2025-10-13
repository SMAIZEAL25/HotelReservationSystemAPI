using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class UserLoggedInEvent : IDomainEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
        public Guid UserId { get; }
        public string Email { get; }

        public UserLoggedInEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
        }
    }
}

