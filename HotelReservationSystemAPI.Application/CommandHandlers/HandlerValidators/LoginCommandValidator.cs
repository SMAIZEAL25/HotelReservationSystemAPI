using FluentValidation;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.CommandHandlers.HandlerValidators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.LoginDto.Email)
                .NotEmpty().WithMessage("Email is required")
                .Must(BeValidEmail).WithMessage("Invalid email format");

            RuleFor(x => x.LoginDto.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");
        }

        private static bool BeValidEmail(string email) => EmailVO.Create(email).IsSuccess;
    }
}
