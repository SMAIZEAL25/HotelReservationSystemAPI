namespace HotelReservationAPI.Infrastructure.Repositories.Interface
{
    public interface IRoleRepository
    {
        Task<APIResponse> GetByNameAsync();
    }
}
