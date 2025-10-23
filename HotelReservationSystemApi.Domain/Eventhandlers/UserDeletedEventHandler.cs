using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Eventhandlers
{
    public class UserDeletedEventHandler : INotificationHandler<UserDeletedEvent>
    {
        private readonly ILogger<UserDeletedEventHandler> _logger;

        public UserDeletedEventHandler(ILogger<UserDeletedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UserDeletedEvent: EventId={EventId}, OccurredAt={OccurredAt}, UserId={UserId}, Email={Email}",
                notification.EventId, notification.OccurredAt, notification.AggregateId, notification.Email);

            // Pure domain side effect: e.g., update related aggregates (no infra)
            // (Side effects like email/cache now in command handler for orchestration)

            return Task.CompletedTask;
        }
    }
}
