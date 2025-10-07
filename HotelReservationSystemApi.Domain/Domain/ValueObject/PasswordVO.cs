
using System.Net;

namespace HotelReservationSystemAPI.Domain.ValueObject;

public class PasswordVO : IEquatable<PasswordVO>
{
    public string HashedValue { get; private set; }

    // Private constructor with parameter
    private PasswordVO(string hashedValue)
    {
        HashedValue = hashedValue;
    }

    // Factory method returns generic Result<T>
    public static Result<PasswordVO> Create(
        string plainPassword,
        Func<string, string> hashFunction)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            return Result<PasswordVO>.Failure("Password is required");

        if (plainPassword.Length < 8)
            return Result<PasswordVO>.Failure("Password must be at least 8 characters");

        // Additional password rules (optional but recommended)
        if (!HasUpperCase(plainPassword))
            return Result<PasswordVO>.Failure("Password must contain at least one uppercase letter");

        if (!HasLowerCase(plainPassword))
            return Result<PasswordVO>.Failure("Password must contain at least one lowercase letter");

        if (!HasDigit(plainPassword))
            return Result<PasswordVO>.Failure("Password must contain at least one digit");

        var hashedPassword = hashFunction(plainPassword);
        return Result<PasswordVO>.Success(
            new PasswordVO(hashedPassword),
            "Password created successfully");
    }

    // Helper methods for password validation
    private static bool HasUpperCase(string password)
        => password.Any(char.IsUpper);

    private static bool HasLowerCase(string password)
        => password.Any(char.IsLower);

    private static bool HasDigit(string password)
        => password.Any(char.IsDigit);

    // Equality implementation
    public override bool Equals(object? obj)
        => obj is PasswordVO other && HashedValue == other.HashedValue;

    public bool Equals(PasswordVO? other)
        => other is not null && HashedValue == other.HashedValue;

    public override int GetHashCode()
        => HashedValue.GetHashCode();

    public override string ToString()
        => "****** (hashed)"; // Never expose the actual hash

    // Operators for convenience
    public static bool operator ==(PasswordVO? left, PasswordVO? right)
        => Equals(left, right);

    public static bool operator !=(PasswordVO? left, PasswordVO? right)
        => !Equals(left, right);
}

