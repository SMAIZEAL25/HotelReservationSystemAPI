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
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var command = new RegisterUserCommand(
                dto.FullName,
                dto.Email,
                dto.Password,
                dto.Role);

            var response = await _mediator.Send(command);

            return response.IsSuccess
                ? StatusCode((int)HttpStatusCode.OK, response)
                : StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var command = new LoginCommand(dto);
            var response = await _mediator.Send(command);
            return response.IsSuccess
                ? Ok(response)
                : Unauthorized(response); // Or StatusCode((int)response.StatusCode, response)
        }
    }
}


