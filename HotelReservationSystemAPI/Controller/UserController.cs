using HotelReservationAPI.Domain.Interface;
using HotelReservationAPI.Infrastructure.Implementation;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net;

namespace HotelReservationSystemAPI.Controller
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Roles = "SuperAdmin")] // RBAC
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EventStore _eventStore;
        private readonly IMediator _mediator;

        public UserController(UserService userService, EventStore eventStore, IMediator mediator)
        {
            _userService = userService;
            _eventStore = eventStore;
            _mediator = mediator;
        }

        [HttpGet("users/{id}/events")]
        public async Task<IActionResult> GetUserEvents(Guid id)
        {
            var events = await _eventStore.GetEventsAsync<UserRegisteredEvent>(id);
            return Ok(events);
        }


        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var response = await _mediator.Send(command);
            return response.IsSuccess
                ? StatusCode((int)HttpStatusCode.Created, response)
                : StatusCode((int)response.StatusCode, response);
        }

        // this endpoint should use a mediator throung the command and handler to talk to the user service 
        // remember for all endpoint in the service use implements ratelimiting conception 


    }
}
