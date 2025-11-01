using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.Eventhandlers;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;

namespace HotelReservationSystemAPI.Application.CommandHandlers
{
    public class AssignRoleHandler : IRequestHandler<AssignRoleCommand, APIResponse<bool>>
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IEventStore _eventStore;
        private readonly IEventBus _eventBus;
        private readonly IMediator _mediator;
        private readonly ILogger<AssignRoleHandler> _logger;

        public AssignRoleHandler(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IEventStore eventStore,
            IEventBus eventBus,
            IMediator mediator,
            ILogger<AssignRoleHandler> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _eventStore = eventStore;
            _eventBus = eventBus;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<APIResponse<bool>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Assigning role {RoleName} to user {UserId}", request.RoleName, request.UserId);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return APIResponse<bool>.Fail(HttpStatusCode.NotFound, "User not found");

            var role = await _roleManager.FindByNameAsync(request.RoleName);
            if (role == null)
                return APIResponse<bool>.Fail(HttpStatusCode.NotFound, "Role not found");

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Role assignment failed for {UserId}: {Errors}", request.UserId, string.Join(", ", result.Errors.Select(e => e.Description)));
                return APIResponse<bool>.Fail(HttpStatusCode.InternalServerError, "Role assignment failed");
            }

            // Add permissions to user claims (for JWT refresh or session)
            await RefreshUserClaimsAsync(user, role);

            // Events (notify with permissions)
            var assignEvent = new RoleAssignedEvent(request.UserId, request.RoleName, role.Permissions);
            await _eventStore.SaveEventAsync(assignEvent);
            await _mediator.Publish(assignEvent, cancellationToken);
            _eventBus.Publish(assignEvent);

            _logger.LogInformation("Role {RoleName} assigned to {UserId}", request.RoleName, request.UserId);
            return APIResponse<bool>.Success(true, "Role assigned successfully");
        }

        private async Task RefreshUserClaimsAsync(User user, Role role)
        {
            // Update user claims with permissions (for JWT)
            foreach (var permission in role.Permissions)
            {
                await _userManager.AddClaimAsync(user, new Claim("Permission", permission));
            }
        }
    }
}