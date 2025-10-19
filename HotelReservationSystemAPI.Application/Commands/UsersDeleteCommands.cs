using HotelReservationSystemAPI.Application.CommonResponse;
using MediatR;

namespace HotelReservationSystemAPI.Application.Commands
{
   
    public record DeleteUserCommand(Guid UserId) : IRequest<APIResponse<bool>>;
}
