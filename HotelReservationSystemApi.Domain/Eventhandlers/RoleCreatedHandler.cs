using HotelReservationSystemAPI.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Domain.Eventhandlers
{
    public class RoleCreatedEventHandler : INotificationHandler<RoleCreatedEvent>
    {
        private readonly ILogger<RoleCreatedEventHandler> _logger;

        public RoleCreatedEventHandler(ILogger<RoleCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(RoleCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("✅ Role created event received: {RoleId} | {RoleName} | {OccurredAt}",
                notification.AggregateId, notification.Name, notification.OccurredAt);

            // Example: sync with external identity provider or audit log
            return Task.CompletedTask;
        }
    }
}