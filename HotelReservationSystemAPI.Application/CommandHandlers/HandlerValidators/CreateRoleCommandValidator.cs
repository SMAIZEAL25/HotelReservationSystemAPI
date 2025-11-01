using FluentValidation;
using HotelReservationSystemAPI.Application.Commands;


namespace HotelReservationSystemAPI.Application.CommandHandlers.HandlerValidators
{

    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .Length(3, 10).WithMessage("Role name must be between 3 and 10 characters")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Role name can only contain letters, numbers, and underscores.")
                .MaximumLength(100)
                .Must(BeValidRoleName).WithMessage("Invalid role name. Allowed: Guest, HotelAdmin, SuperAdmin");
        }

        private static bool BeValidRoleName(string name)
        {
            var allowed = new[] { "Guest", "HotelAdmin", "SuperAdmin" };
            return allowed.Contains(name, StringComparer.OrdinalIgnoreCase);
        }
    }




    //public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    //{
    //    public CreateRoleCommandValidator()
    //    {
    //        RuleFor(x => x.Name)
    //            .NotEmpty().WithMessage("Role name is required")
    //            .Length(3, 50).WithMessage("Role name must be between 3 and 50 characters")
    //            .Must(BeValidRoleName).WithMessage("Invalid role name. Allowed: Guest, HotelAdmin, SuperAdmin");
    //    }

    //    private static bool BeValidRoleName(string name)
    //    {
    //        var allowed = new[] { "Guest", "HotelAdmin", "SuperAdmin" };
    //        return allowed.Contains(name, StringComparer.OrdinalIgnoreCase);
    //    }
    //}
}

