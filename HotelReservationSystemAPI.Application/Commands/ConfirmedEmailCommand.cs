using HotelReservationSystemAPI.Application.CommonResponse;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Commands
{
    public record ConfirmEmailCommand(string Email, string Token) : IRequest<APIResponse<bool>>;
}
