using HotelReservationSystemAPI.Application.CommonResponse;

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

        public static APIResponse<RoleValue> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !AllowedRoles.Contains(name))
                return APIResponse<RoleValue>.Fail(HttpStatusCode.BadRequest, $"Invalid role: {name}");

            return APIResponse<RoleValue>.Success(new RoleValue(name), "Role created successfully");
        }

        public bool IsAdmin() => Name is "HotelAdmin" or "SuperAdmin";
        public bool IsGuest() => Name == "Guest";

        public bool Equals(RoleValue? other) => other is not null && Name == other.Name;
        public override bool Equals(object? obj) => Equals(obj as RoleValue);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }
}
