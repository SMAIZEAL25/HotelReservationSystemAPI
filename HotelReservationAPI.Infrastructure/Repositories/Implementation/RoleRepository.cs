using HotelReservationAPI.Infrastructure.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Repositories.Implementation
{
    public class RoleRepository : IRoleRepository
    {

        public Task<APIResponse> GetByNameAsync()
        {
            throw new NotImplementedException();
        }
    }

    // Policies for authentication and authorization  as a guest manager and admin 
}
