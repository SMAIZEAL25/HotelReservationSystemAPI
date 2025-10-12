
using HotelReservationSystemAPI.Domain.Events;
using HotelReservationSystemAPI.Domain.ValueObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;


namespace HotelReservationSystemAPI.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = string.Empty;
        public EmailVO EmailValueObject { get; private set; } = default!;
        public PasswordVO? PasswordValueObject { get; private set; }
        public UserRole Role { get; private set; } = UserRole.Guest;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; private set; } = false;

        private User() { } // EF

        public static Result<UserCreationData> Create(
            string fullName,
            string email,
            string plainPassword,
            UserRole role,
            Func<string, string> hashFunction)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Result<UserCreationData>.Failure("Full name is required");

            var emailResult = EmailVO.Create(email);
            if (!emailResult.IsSuccess)
                return Result<UserCreationData>.Failure(emailResult.Error ?? "Invalid email");

            var passwordResult = PasswordVO.Create(plainPassword, hashFunction);
            if (!passwordResult.IsSuccess)
                return Result<UserCreationData>.Failure(passwordResult.Error ?? "Invalid password");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                UserName = email,
                Email = email,
                EmailValueObject = emailResult.Value,
                PasswordValueObject = passwordResult.Value,
                PasswordHash = passwordResult.Value.HashedValue,
                Role = role,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var domainEvent = new UserRegisteredEvent(user.Id, email);
            return Result<UserCreationData>.Success(new UserCreationData(user, domainEvent));
        }

        public OperationResult ConfirmEmail()
        {
            if (EmailConfirmed)
                return OperationResult.Failure("Email already confirmed");
            EmailConfirmed = true;
            return OperationResult.Success("Email confirmed successfully");
        }

        public OperationResult UpdateProfile(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return OperationResult.Failure("Full name is required");

            var emailResult = EmailVO.Create(email);
            if (!emailResult.IsSuccess)
                return OperationResult.Failure(emailResult.Error ?? "Invalid email");

            FullName = fullName;
            EmailValueObject = emailResult.Value;
            Email = email;
            UserName = email;
            return OperationResult.Success("Profile updated successfully");
        }

        public OperationResult ChangePassword(string newPassword, Func<string, string> hashFunction)
        {
            var passwordResult = PasswordVO.Create(newPassword, hashFunction);
            if (!passwordResult.IsSuccess)
                return OperationResult.Failure(passwordResult.Error ?? "Invalid password");

            PasswordValueObject = passwordResult.Value;
            PasswordHash = passwordResult.Value.HashedValue;
            return OperationResult.Success("Password changed successfully");
        }
    }

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

    public enum UserRole
    {
        Guest,
        HotelAdmin,
        SuperAdmin
    }

}






