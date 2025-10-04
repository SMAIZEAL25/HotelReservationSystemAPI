namespace HotelReservationAPI.Infrastructure.Repositories.Interface
{
    public interface IUserRepository
    {
       Task <APIResponse> GetByEmailAsync(UserRegisterDto userRegisterDto);
    }
}
