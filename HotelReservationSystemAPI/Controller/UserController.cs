using HotelReservationAPI.Application.Interface;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StackExchange.Redis;
using System.Net;

namespace HotelReservationSystemAPI.Controller
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEventStore _eventStore;  // Abstraction for events

        public UsersController(IMediator mediator, IEventStore eventStore)
        {
            _mediator = mediator;
            _eventStore = eventStore;
        }

        [HttpGet("{id}/events")]
        [Authorize(Roles = "SuperAdmin" , Policy = "")]
        public async Task<IActionResult> GetUserEvents(Guid id)
        {
            var events = await _eventStore.GetEventsAsync<UserRegisteredEvent>(id);  // Direct call for events
            return Ok(events);
        }

       

        [HttpDelete("{id}")]
        [Authorize (Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var command = new DeleteUserCommand(id);
            var response = await _mediator.Send(command);  // → Handler → Repo SoftDeleteAsync
            return response.IsSuccess ? NoContent() : BadRequest(response);
        }
    }

}

