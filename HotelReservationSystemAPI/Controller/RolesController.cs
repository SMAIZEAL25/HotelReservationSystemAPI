using HotelReservationSystemAPI.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservationSystemAPI.Controller
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "SuperAdmin")]  // RBAC for admin only
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var response = await _mediator.Send(command);  // → Handler (uses RoleManager + repo if needed)
            return response.IsSuccess ? CreatedAtAction(nameof(CreateRole), response) : BadRequest(response);
        }
    }

}

