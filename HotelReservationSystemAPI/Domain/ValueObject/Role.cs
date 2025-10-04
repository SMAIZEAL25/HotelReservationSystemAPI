using HotelReservationSystemAPI.Domain.CommonrResponse;
using System.Net;

namespace HotelReservationSystemAPI.Domain.ValueObject
{
    public sealed class Role : IEquatable<Role>
    {
        public string Name { get; private set; } = string.Empty;

        private static readonly HashSet<string> AllowedRoles = new()
    {
        "Guest", "HotelAdmin", "SuperAdmin"
    };

        private Role(string name) => Name = name;

        public static APIResponse<Role> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || !AllowedRoles.Contains(name))
                return APIResponse<Role>.Fail(HttpStatusCode.BadRequest, $"Invalid role: {name}");

            return APIResponse<Role>.Success(new Role(name), "Role created successfully");
        }

        public bool IsAdmin() => Name is "HotelAdmin" or "SuperAdmin";
        public bool IsGuest() => Name == "Guest";

        public bool Equals(Role? other) => other is not null && Name == other.Name;
        public override bool Equals(object? obj) => Equals(obj as Role);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => Name;
    }
}
