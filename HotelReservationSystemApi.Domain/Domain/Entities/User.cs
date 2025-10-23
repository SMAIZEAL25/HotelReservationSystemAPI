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
        public bool IsDeleted { get; private set; } = false;  // Soft delete flag
        public DateTime? DeletedAt { get; private set; }  // Audit timestamp        
        public string? RefreshToken { get; private set; } // For refresh token storage
        public DateTime? RefreshTokenExpiry { get; private set; } // Refresh token expiry
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
            if (!passwordResult.IsSuccess || passwordResult.Value == null)
                return Result<UserCreationData>.Failure(passwordResult.Error ?? "Invalid password");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                UserName = email,
                Email = email,
                EmailValueObject = emailResult.Value!,
                PasswordValueObject = passwordResult.Value!,
                PasswordHash = passwordResult.Value!.HashedValue, // <-- Add ! to suppress null warning
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

        public OperationResult UpdateRefreshToken(string newToken, TimeSpan lifetime)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                return OperationResult.Failure("Refresh token cannot be empty.");

            RefreshToken = newToken;
            RefreshTokenExpiry = DateTime.UtcNow.Add(lifetime);
            return OperationResult.Success("Refresh token updated successfully.");
        }

        public OperationResult RevokeRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpiry = null;
            return OperationResult.Success("Refresh token revoked.");
        }

        public OperationResult UpdateProfile(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return OperationResult.Failure("Full name is required");

            var emailResult = EmailVO.Create(email);
            if (!emailResult.IsSuccess)
                return OperationResult.Failure(emailResult.Error ?? "Invalid email");

            FullName = fullName;
            EmailValueObject = emailResult.Value!;
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
            PasswordHash = passwordResult.Value != null? passwordResult.Value.HashedValue : string.Empty;
            return OperationResult.Success("Password changed successfully");
        }

        // Soft Deletion
        public OperationResult SoftDelete()
        {
            if (IsDeleted)
                return OperationResult.Failure("User already deleted.");
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            return OperationResult.Success("User soft deleted successfully.");
        }

        // Recovery
        public OperationResult Recover()
        {
            if (!IsDeleted)
                return OperationResult.Failure("User not deleted.");
            IsDeleted = false;
            DeletedAt = null;
            return OperationResult.Success("User recovered successfully.");
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











