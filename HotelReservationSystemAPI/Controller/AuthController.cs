using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var command = new RegisterUserCommand(
                dto.FullName,
                dto.Email,
                dto.Password,
                dto.Phonenumber,
                dto.Role);

            var response = await _mediator.Send(command);

            return response.IsSuccess
                ? StatusCode((int)HttpStatusCode.OK, response)
                : StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("login")]
        [AllowAnonymous]

        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var command = new LoginCommand(dto);
            var response = await _mediator.Send(command);
            return response.IsSuccess
                ? Ok(response)
                : Unauthorized(response); 
        }

        [HttpGet("confirm-email")]

        public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {
            var command = new ConfirmEmailCommand(email, token);  // Map query to command
            var response = await _mediator.Send(command);  // → Handler (no repo direct call; handler uses UserManager + repo)
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("refresh")]
        [Authorize(Policy = "RequireGuest")]  // Enforces "read:profile" permission from Guest role
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var command = new RefreshTokenCommand(dto.RefreshToken);
            var response = await _mediator.Send(command);
            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }
    }
}


