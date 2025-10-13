using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Eventhandlers
{
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredEventHandler> _logger;
        // Inject other services (e.g., IEmailService)

        public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UserRegisteredEvent: EventId={EventId}, OccurredAt={OccurredAt}, UserId={UserId}, Email={Email}",
                notification.EventId, notification.OccurredAt, notification.UserId, notification.Email);

            // Side effect: e.g., SendWelcomeEmail(notification.Email)
            // Or integrate with other contexts via _eventBus

            return Task.CompletedTask;
        }
    }
}