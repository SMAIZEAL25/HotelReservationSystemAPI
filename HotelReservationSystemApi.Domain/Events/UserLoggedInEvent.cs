using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public class UserLoggedInEvent : INotification
    {
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime OccurredAt { get; }

        public UserLoggedInEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
            OccurredAt = DateTime.UtcNow;
        }
    }
}

