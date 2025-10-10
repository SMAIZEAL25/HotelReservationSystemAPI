using HotelReservationAPI.Domain.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
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
        private readonly ILogger<ConfirmEmailCommandHandler> _logger;
        private readonly IEventStore _eventStore;
        private readonly IMediator _mediator;

        public ConfirmEmailCommandHandler(
            UserManager<User> userManager,
            ILogger<ConfirmEmailCommandHandler> logger,
            IEventStore eventStore,
            IMediator mediator)
        {
            _userManager = userManager;
            _logger = logger;
            _eventStore = eventStore;
            _mediator = mediator;
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

            var isValidToken = await _userManager.VerifyEmailAsync(user, request.Token);
            if (!isValidToken.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed: invalid token for {Email}", request.Email);
                return APIResponse<bool>.Fail(HttpStatusCode.BadRequest, "Invalid confirmation token.");
            }

            // Update via domain behavior
            user.ConfirmEmail(); // Assumes User entity has this method for invariants
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update email confirmation for {Email}: {Errors}", request.Email,
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Failed to confirm email.");
            }

            // Raise domain event for confirmation (part of flow)
            var confirmEvent = new EmailConfirmedEvent(user.Id, user.Email);
            await _eventStore.SaveEventAsync(confirmEvent);
            await _mediator.Publish(confirmEvent);

            _logger.LogInformation("Email confirmed successfully for {Email}", request.Email);
            return APIResponse<bool>.Success(true, "Email confirmed successfully.");
        }
    }
}
