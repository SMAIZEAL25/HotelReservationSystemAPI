

using System.Net;

namespace HotelReservationSystemAPI.Domain.ValueObject
{
    public sealed class RoleValue : IEquatable<RoleValue>
    {
        public string Name { get; private set; } = string.Empty;
        private static readonly HashSet<string> AllowedRoles = new()
    {
        "Guest", "HotelAdmin", "SuperAdmin"
    };
        private RoleValue(string name) => Name = name;
        public static RoleValueResult Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !AllowedRoles.Contains(name))
                return RoleValueResult.Failure($"Invalid role: {name}");
            return RoleValueResult.Success(new RoleValue(name));
        }
        public bool IsAdmin() => Name is "HotelAdmin" or "SuperAdmin";
        public bool IsGuest() => Name == "Guest";
        public bool Equals(RoleValue? other) => other is not null && Name == other.Name;
        public override bool Equals(object? obj) => Equals(obj as RoleValue);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }

    // Domain result for RoleValue
    public class RoleValueResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public RoleValue? Data { get; private set; }
        private RoleValueResult() { }
        public static RoleValueResult Success(RoleValue roleValue)
        {
            return new RoleValueResult
            {
                IsSuccess = true,
                Message = "Role created successfully",
                Data = roleValue
            };
        }
        public static RoleValueResult Failure(string message)
        {
            return new RoleValueResult
            {
                IsSuccess = false,
                Message = message
            };
        }
    }
}

