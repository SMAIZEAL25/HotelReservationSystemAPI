using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Events
{
    public interface IDomainEvent : INotification
    {
        Guid EventId { get; }
        DateTime OccurredAt { get; }
    }
}
