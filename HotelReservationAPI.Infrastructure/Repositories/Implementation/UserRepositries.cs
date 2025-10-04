using HotelReservationAPI.Infrastructure.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservationAPI.Infrastructure.Repositories.Implementation
{
    public class UserRepositroy : IUserRepository
    {
        public Task<APIResponse> GetByEmailAsync(UserRegisterDto userRegisterDto)
        {
            throw new NotImplementedException();
        }
    }
}
