using HotelReservationSystemAPI.Domain.Entities;

namespace HotelReservationAPI.Domain.Interface
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> ListAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
