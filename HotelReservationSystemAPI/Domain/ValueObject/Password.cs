

using HotelReservationSystemAPI.Domain.CommonrResponse;
using System.Net;

namespace HotelReservationSystemAPI.Domain.ValueObject;

public sealed class Password
{
    public string Hash { get; private set; } = string.Empty;

    private Password(string hash) => Hash = hash;

    public static APIResponse<Password> Create(string plainPassword, Func<string, string> hashFunction)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || plainPassword.Length < 6)
            return APIResponse<Password>.Fail(HttpStatusCode.BadRequest, "Password must be at least 6 characters long.");

        var hashed = hashFunction(plainPassword);
        return APIResponse<Password>.Success(new Password(hashed), "Password created successfully");
    }

    public bool Verify(string plainPassword, Func<string, string, bool> verifyFunction) =>
        verifyFunction(plainPassword, Hash);
    
    public override string ToString() => "[PROTECTED]";
}
