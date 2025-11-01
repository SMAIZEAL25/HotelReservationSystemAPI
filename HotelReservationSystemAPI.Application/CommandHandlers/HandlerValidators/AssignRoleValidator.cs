using FluentValidation;
using HotelReservationSystemAPI.Application.Commands;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(BeAValidRole)
            .WithMessage("Invalid or unrecognized role.");
    }

    private bool BeAValidRole(string roleName)
    {
        var validRoles = new[] { "Guest", "HotelAdmin", "SuperAdmin" };
        return validRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}
