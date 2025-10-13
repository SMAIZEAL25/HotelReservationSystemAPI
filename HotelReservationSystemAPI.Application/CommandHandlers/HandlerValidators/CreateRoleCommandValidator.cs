using FluentValidation;
using HotelReservationSystemAPI.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.CommandHandlers.HandlerValidators
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .Length(3, 50).WithMessage("Role name must be between 3 and 50 characters")
                .Must(BeValidRoleName).WithMessage("Invalid role name. Allowed: Guest, HotelAdmin, SuperAdmin");
        }

        private static bool BeValidRoleName(string name)
        {
            var allowed = new[] { "Guest", "HotelAdmin", "SuperAdmin" };
            return allowed.Contains(name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
