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
    public class LoginCommand : IRequest<APIResponse<AuthResponseDto>>
    {
        public UserLoginDto LoginDto { get; }

        public LoginCommand(UserLoginDto dto)
        {
            LoginDto = dto;
        }
    }
}
