// LoginCommand.cs
using HotelReservationSystemAPI.Application.CommonResponse;
using MediatR;

namespace HotelReservationSystemAPI.Application.Commands;

public record LoginCommand(UserLoginDto LoginDto) : IRequest<APIResponse<LoginResponseDto>>;