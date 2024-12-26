using YourDriver.Controllers.DTOS;
using YourDriver.Model.AuthMangment;
using YourDriver.Model.DriversData;
using YourDriver.Model.PassengersData;

namespace YourDriver.AuthServices
{
    public interface IAuthService
    {
        Task<AuthModel> RegiserPassenger(PassengerRegister newPassenger);
        Task<AuthModel> RegiserDriver(DriverRegister newDriver);
        Task<AuthModel> Login(LoginDTO user);
        Task<DriverUpdateModel> UpdateDriver(DriverUpdateModel newDriver, string driverId);
        Task<PassengerUpdateModel> UpdatePassenger(PassengerUpdateModel newPassenger, string passengerId);
        Task<AuthModel> RefreshToken(string token);
        Task<bool> RevokeRefreshToken(string token);
        Task<bool> DeleteUser(string name);
    }
}
