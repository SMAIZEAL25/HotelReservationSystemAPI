using HotelReservationSystemAPI.Application.CommonResponse;
using MediatR;

namespace HotelReservationSystemAPI.Application.Commands
{
    public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest<APIResponse<bool>>;
}
