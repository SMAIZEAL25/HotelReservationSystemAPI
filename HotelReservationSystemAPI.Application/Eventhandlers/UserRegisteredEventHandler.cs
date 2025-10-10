using HotelReservationSystemAPI.Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Eventhandlers
{
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ User registered event received: {UserId} | {Email} | {OccurredAt}",
                notification.UserId, notification.Email, notification.OccurredAt);

            // Example: trigger welcome email, audit log, or analytics hook
            return Task.CompletedTask;
        }
    }
}
