using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<APIResponse<LoginResponseDto>>;
}
