using FluentValidation;
using HotelReservationSystemAPI.Application.Commands;
using HotelReservationSystemAPI.Domain.Entities;
using HotelReservationSystemAPI.Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.CommandHandlers.HandlerValidators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .Must(BeValidEmail).WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Must(HaveUppercase).WithMessage("Password must contain at least one uppercase letter")
                .Must(HaveLowercase).WithMessage("Password must contain at least one lowercase letter")
                .Must(HaveDigit).WithMessage("Password must contain at least one digit");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(BeValidRole).WithMessage("Invalid role. Allowed: Guest, HotelAdmin, SuperAdmin");
        }

        private static bool BeValidEmail(string email) => EmailVO.Create(email).IsSuccess;
        private static bool HaveUppercase(string password) => password.Any(char.IsUpper);
        private static bool HaveLowercase(string password) => password.Any(char.IsLower);
        private static bool HaveDigit(string password) => password.Any(char.IsDigit);
        private static bool BeValidRole(string role) => Enum.TryParse<UserRole>(role, true, out _);
    }
}
