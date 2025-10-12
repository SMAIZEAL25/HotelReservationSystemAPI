// RegisterUserCommand.cs
using HotelReservationSystemAPI.Application.CommonResponse;
using HotelReservationSystemAPI.Application.DTO_s;
using MediatR;
using System.Net;

namespace HotelReservationSystemAPI.Application.Commands;

public record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role = "Guest") : IRequest<APIResponse<UserDto>>;