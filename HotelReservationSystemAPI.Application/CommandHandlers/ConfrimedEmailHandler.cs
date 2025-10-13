using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.CommandHandlers
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, APIResponse<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;  // For update if needed
        private readonly IEventStore _eventStore;  // Optional persistence
        private readonly IEventBus _eventBus;  // Custom bus
        private readonly IMediator _mediator;
        private readonly ILogger<ConfirmEmailCommandHandler> _logger;

        public ConfirmEmailCommandHandler(
            UserManager<User> userManager,
            IUserRepository userRepository,
            IEventStore eventStore,
            IEventBus eventBus,
            IMediator mediator,
            ILogger<ConfirmEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _eventStore = eventStore;
            _eventBus = eventBus;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<APIResponse<bool>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Confirming email for {Email}", request.Email);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation failed: user not found for {Email}", request.Email);
                return APIResponse<bool>.Fail(HttpStatusCode.NotFound, "User not found.");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for {Email}", request.Email);
                return APIResponse<bool>.Success(true, "Email already confirmed.");
            }

            var isValidToken = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!isValidToken.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed: invalid token for {Email}", request.Email);
                return APIResponse<bool>.Fail(HttpStatusCode.BadRequest, "Invalid confirmation token.");
            }

            // Update via domain behavior
            var confirmResult = user.ConfirmEmail();  // Domain method
            if (!confirmResult.IsSuccess)
            {
                _logger.LogError("Domain confirmation failed for {Email}: {Error}", request.Email, confirmResult.Error);
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Failed to confirm email.");
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update email confirmation for {Email}: {Errors}", request.Email,
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Failed to confirm email.");
            }

            // Events: Store → MediatR → Bus
            var confirmEvent = new EmailConfirmedEvent(user.Id, user.Email);
            await _eventStore.SaveEventAsync(confirmEvent);  // Optional persistence
            await _mediator.Publish(confirmEvent, cancellationToken);  // Triggers handlers (e.g., logging)
            _eventBus.Publish(confirmEvent);  // Custom bus for downstream

            _logger.LogInformation("Email confirmed successfully for {Email}", request.Email);
            return APIResponse<bool>.Success(true, "Email confirmed successfully.");
        }
    }
}