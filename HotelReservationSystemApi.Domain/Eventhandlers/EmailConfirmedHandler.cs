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
    public class EmailConfirmedEventHandler : INotificationHandler<EmailConfirmedEvent>
    {
        private readonly ILogger<EmailConfirmedEventHandler> _logger;
        // Inject e.g., IEmailService for follow-up emails

        public EmailConfirmedEventHandler(ILogger<EmailConfirmedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(EmailConfirmedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling EmailConfirmedEvent: EventId={EventId}, OccurredAt={OccurredAt}, UserId={UserId}, Email={Email}",
                notification.EventId, notification.OccurredAt, notification.UserId, notification.Email);

            // Side effect: e.g., SendWelcomeEmail(notification.Email)
            // Or notify other contexts via _eventBus

            return Task.CompletedTask;
        }
    }
}
