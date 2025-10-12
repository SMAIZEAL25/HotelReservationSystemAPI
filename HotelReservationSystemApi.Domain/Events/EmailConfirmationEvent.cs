using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class EmailConfirmedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public string Email { get; }
        public EmailConfirmedEvent(Guid userId, string email) { UserId = userId; Email = email; }
    }
}
