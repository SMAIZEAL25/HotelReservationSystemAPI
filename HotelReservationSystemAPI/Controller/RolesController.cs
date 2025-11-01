using HotelReservationSystemAPI.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservationSystemAPI.Controller
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdminOrHigher")]  // Policy: SuperAdmin or HotelAdmin
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var response = await _mediator.Send(command);
            return response.IsSuccess
                ? CreatedAtAction(nameof(CreateRole), response)
                : BadRequest(response);
        }

        [HttpPost("assign")]
        [Authorize(Policy = "RequireSuperAdmin")]  // Policy: SuperAdmin only (strict)
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
        {
            var response = await _mediator.Send(command);
            return response.IsSuccess
                ? Ok(response)
                : BadRequest(response);
        }
    }
}

