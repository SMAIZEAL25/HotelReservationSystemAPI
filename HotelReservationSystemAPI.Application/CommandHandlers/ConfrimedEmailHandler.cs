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
        private readonly IUserRepository _userRepository;
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;
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

            var confirmResult = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!confirmResult.Succeeded)
            {
                _logger.LogWarning("Email confirmation failed: invalid token for {Email}", request.Email);
                return APIResponse<bool>.Fail(HttpStatusCode.BadRequest, "Invalid confirmation token.");
            }

            // Update via domain behavior
            var domainConfirmResult = user.ConfirmEmail();
            if (!domainConfirmResult.IsSuccess)
            {
                _logger.LogError("Domain confirmation failed for {Email}: {Error}", request.Email, domainConfirmResult.Error);
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Failed to confirm email.");
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to update email confirmation for {Email}: {Errors}", request.Email,
                    string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Failed to confirm email.");
            }

            // Events
            var confirmEvent = new EmailConfirmedEvent(user.Id, user.Email);
            await _eventStore.SaveEventAsync(confirmEvent);
            await _mediator.Publish(confirmEvent, cancellationToken);
            _eventBus.Publish(confirmEvent);

            _logger.LogInformation("Email confirmed successfully for {Email}", request.Email);
            return APIResponse<bool>.Success(true, "Email confirmed successfully.");
        }
    }
}