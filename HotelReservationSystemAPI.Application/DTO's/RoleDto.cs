using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationSystemAPI.Application.DTO_s
{
    public class RoleDto
    {
        public string Name { get; set; } = string.Empty;

        public static RoleDto FromRole(Role role) => new() { Name = role.Name };
    }
}
