using HotelReservationAPI.Domain.Interface;
using HotelReservationAPI.Infrastructure.Implementation;
using HotelReservationSystemAPI.Application.DTO_s;
using HotelReservationSystemAPI.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HotelReservationSystemAPI.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly EventStore _eventStore;

        public UserController(UserService userService, EventStore eventStore)
        {
            _userService = userService;
            _eventStore = eventStore;
        }

        [HttpGet("users/{id}/events")]
        public async Task<IActionResult> GetUserEvents(Guid id)
        {
            var events = await _eventStore.GetEventsAsync<UserRegisteredEvent>(id);
            return Ok(events);
        }


        // this endpoint should use a mediator throung the command and handler to talk to the user service 
        // remember for all endpoint in the service use implements ratelimiting conception 
      

    }
}
