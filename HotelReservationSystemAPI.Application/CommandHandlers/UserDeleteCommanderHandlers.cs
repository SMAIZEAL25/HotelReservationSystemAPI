using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.CommandHandlers
{


    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, APIResponse<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;
        private readonly IMediator _mediator;
        private readonly IEmailService _emailService;  // Injected here for side effects (Application orchestration)
        private readonly IDistributedCache _cache;  // Injected here for cache invalidation
        private readonly ILogger<DeleteUserCommandHandler> _logger;

        public DeleteUserCommandHandler(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IEventStore eventStore,
            IEventBus eventBus,
            IMediator mediator,
            IEmailService emailService,  // Added: For admin notification
            IDistributedCache cache,  // Added: For cache invalidation
            ILogger<DeleteUserCommandHandler> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _eventStore = eventStore;
            _eventBus = eventBus;
            _mediator = mediator;
            _emailService = emailService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<APIResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Attempting to soft delete user {UserId}", request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found for soft delete: {UserId}", request.UserId);
                return APIResponse<bool>.Fail(HttpStatusCode.NotFound, "User not found.");
            }

            // Domain behavior
            var result = user.SoftDelete();
            if (!result.IsSuccess)
            {
                _logger.LogError("Domain soft delete failed for {UserId}: {Error}", request.UserId, result.Error);
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, result.Error?? "unknown Error");
            }

            // Persist via repo (updates IsDeleted/DeletedAt)
            await _userRepository.SoftDeleteAsync(request.UserId);

            // Events (pure: store → MediatR → bus)
            var deleteEvent = new UserDeletedEvent(request.UserId, user.Email ?? "");  

            await _eventStore.SaveEventAsync(deleteEvent);
            await _mediator.Publish(deleteEvent, cancellationToken);
            _eventBus.Publish(deleteEvent);
            // Side Effects in Handler (Application orchestration – aligns with previous caching/email patterns)
            // 1. Invalidate user cache
            var cacheKey = $"user:{request.UserId}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("User cache invalidated for {UserId}", request.UserId);

            // 2. Notify admin via email (aligns with SendConfirmationEmailAsync usage)
            await _emailService.SendConfirmationEmailAsync(
                "admin@hotelapi.com",  // From config
                "User Account Soft Deleted",
                $"User {user.Email} (ID: {request.UserId}) was soft deleted. Review if needed.");

            _logger.LogInformation("Admin notification sent for deleted user {UserId}", request.UserId);

            _logger.LogInformation("User {UserId} soft deleted successfully", request.UserId);
            return APIResponse<bool>.Success(true, "User soft deleted successfully.");
        }
    }
}

