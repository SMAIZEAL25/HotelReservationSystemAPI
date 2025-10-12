
using System.Net;

namespace HotelReservationSystemAPI.Domain.ValueObject;

public class PasswordVO : ValueObject
{
    public string HashedValue { get; private set; } = string.Empty;

    private PasswordVO(string hashedValue) => HashedValue = hashedValue;

    public static Result<PasswordVO> Create(string plainPassword, Func<string, string> hashFunction)
    {
        if (string.IsNullOrWhiteSpace(plainPassword))
            return Result<PasswordVO>.Failure("Password is required");

        if (plainPassword.Length < 8)
            return Result<PasswordVO>.Failure("Password must be at least 8 characters");

        if (!plainPassword.Any(char.IsUpper))
            return Result<PasswordVO>.Failure("Password must contain at least one uppercase letter");

        if (!plainPassword.Any(char.IsLower))
            return Result<PasswordVO>.Failure("Password must contain at least one lowercase letter");

        if (!plainPassword.Any(char.IsDigit))
            return Result<PasswordVO>.Failure("Password must contain at least one digit");

        var hashedPassword = hashFunction(plainPassword);
        return Result<PasswordVO>.Success(new PasswordVO(hashedPassword));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return HashedValue;
    }
}
