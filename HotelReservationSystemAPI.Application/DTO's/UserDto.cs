using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::HotelReservationSystemAPI.Domain.Entities;

namespace HotelReservationSystemAPI.Application.DTO_s
{

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Factory method to map from User entity (for consistency with domain factory patterns)
        public static UserDto FromUser(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty, // Or user.EmailValueObject.Value for value object consistency
                UserName = user.UserName ?? string.Empty,
                Role = user.Role,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

