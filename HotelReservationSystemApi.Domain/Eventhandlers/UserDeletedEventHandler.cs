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
        private readonly IEmailService _emailService;  // Optional: For admin notifications
        private readonly IDistributedCache _cache;  // Optional: Invalidate user cache

        public UserDeletedEventHandler(
            ILogger<UserDeletedEventHandler> logger,
            IEmailService emailService = null,  // Optional injection
            IDistributedCache cache = null)
        {
            _logger = logger;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UserDeletedEvent: EventId={EventId}, OccurredAt={OccurredAt}, UserId={UserId}, Email={Email}",
                notification.EventId, notification.OccurredAt, notification.UserId, notification.Email);

            // Side effect 1: Invalidate user cache (aligns with registration caching)
            if (_cache != null)
            {
                var cacheKey = $"user:{notification.UserId}";
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                _logger.LogInformation("User cache invalidated for {UserId}", notification.UserId);
            }

            // Side effect 2: Notify admin via email (optional, aligns with IEmailService usage)
            if (_emailService != null)
            {
                await _emailService.SendConfirmationEmailAsync(
                    "admin@hotelapi.com",  // Admin email from config
                    "User Account Deleted",
                    $"User {notification.Email} (ID: {notification.UserId}) was soft deleted at {notification.OccurredAt}. Review if needed.");
                _logger.LogInformation("Admin notification sent for deleted user {UserId}", notification.UserId);
            }

            // Side effect 3: Integrate with other contexts via _eventBus (aligns with previous publishing)
            // e.g., _eventBus.Publish(notification);  // For Notification bounded context

            // Align with previous handlers: Return completed task for fire-and-forget
            return Task.CompletedTask;
        }
    }
}
