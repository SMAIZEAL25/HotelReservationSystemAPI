using HotelReservationAPI.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Implementation
{
    public class EventStore : IEventStore
    {
        public Task<IEnumerable<T>> GetEventsAsync<T>(Guid aggregateId)
        {
            throw new NotImplementedException();
        }

        public Task SaveEventAsync<T>(T @event) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
