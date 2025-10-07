
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;


namespace HotelReservationSystemAPI.Domain.Entities
{
    public partial class User : IdentityUser<Guid>
    {
        // Domain properties
        public string FullName { get; private set; } = string.Empty;
        public EmailVO EmailValueObject { get; private set; } = default!;
        public PasswordVO PasswordValueObject { get; private set; } = default!;
        public UserRole Role { get; private set; }
        public bool EmailConfirmed { get; private set; } = false;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private User() { } // EF Core

        // Factory method - Returns Result<UserCreationData>
        public static Result<UserCreationData> Create(
            string fullName,
            string email,
            string plainPassword,
            UserRole role,
            Func<string, string> hashFunction)
        {
            // Validate full name
            if (string.IsNullOrWhiteSpace(fullName))
                return Result<UserCreationData>.Failure("Full name is required");

            // Validate email
            var emailResult = EmailVO.Create(email);
            if (!emailResult.IsSuccess)
                return Result<UserCreationData>.Failure(emailResult.Message);

            // Validate password
            var passwordResult = PasswordVO.Create(plainPassword, hashFunction);
            if (!passwordResult.IsSuccess)
                return Result<UserCreationData>.Failure(passwordResult.Message);

            // Create user instance
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                UserName = email,
                EmailValueObject = emailResult.Data!,
                PasswordValueObject = passwordResult.Data!,
                PasswordHash = passwordResult.Data!.HashedValue,
                Role = role,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            // Set IdentityUser.Email property
            user.Email = email;

            // Create domain event
            var domainEvent = new UserRegisteredEvent(user.Id, email, DateTime.UtcNow);

            // Return with UserCreationData wrapper
            var creationData = new UserCreationData(user, domainEvent);
            return Result<UserCreationData>.Success(creationData, "User created successfully");
        }

        // This method handles Email confirmations
        public OperationResult ConfirmEmail()
        {
            if (EmailConfirmed)
                return OperationResult.Failure("Email is already confirmed");

            EmailConfirmed = true;
            return OperationResult.Success("Email confirmed successfully");
        }

        // This method handles Updating of profile
        public OperationResult UpdateProfile(string fullName, string email)
        {
            // Validate full name
            if (string.IsNullOrWhiteSpace(fullName))
                return OperationResult.Failure("Full name is required");

            // Validate email
            var emailResult = EmailVO.Create(email);
            if (!emailResult.IsSuccess)
                return OperationResult.Failure(emailResult.Message);

            // Update properties
            FullName = fullName;
            EmailValueObject = emailResult.Data!;
            UserName = email;
            base.Email = emailResult.Data!.Value;

            return OperationResult.Success("Profile updated successfully");
        }

        // This method handles the ChangePassword
        public OperationResult ChangePassword(
            string newPlainPassword,
            Func<string, string> hashFunction)
        {
            var passwordResult = PasswordVO.Create(newPlainPassword, hashFunction);
            if (!passwordResult.IsSuccess)
                return OperationResult.Failure(passwordResult.Message);

            PasswordValueObject = passwordResult.Data!;
            PasswordHash = passwordResult.Data!.HashedValue;

            return OperationResult.Success("Password changed successfully");
        }
    }

    // ===== DATA WRAPPER =====
    public class UserCreationData
    {
        public User User { get; }
        public UserRegisteredEvent DomainEvent { get; }

        public UserCreationData(User user, UserRegisteredEvent domainEvent)
        {
            User = user;
            DomainEvent = domainEvent;
        }
    }

    // ===== ENUMS =====
    public enum UserRole
    {
        Guest = 0,
        HotelAdmin = 1,
        SuperAdmin = 2
    }

    // ===== DOMAIN EVENTS =====
    public record UserRegisteredEvent(Guid UserId, string Email, DateTime OccurredAt);
}

    




