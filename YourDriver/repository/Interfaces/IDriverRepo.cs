using YourDriver.Model.DriversData;
using YourDriver.Model.NotificationsAndOffers;
using YourDriver.Model.PassengersData;
using YourDriver.Model.Tripes;

namespace YourDriver.repository.Interfaces
{
    public interface IDriverRepo : IBaseRepo<Driver>
    {
        Task<DriverUpdateModel> UpdateDriver(DriverUpdateModel newDriver, string driverId);
        Task<List<string>> AddNewFavArea(string area, string driverId);
        Task<bool> DeleteArea(string area, string driverId);
        Task UpdateNotificationAsync(Driver driver, string Location, string Destination, Passenger passenger);
        Task<List<NotificationModel>> ShowNotificationAsync(string driverId);
        Task<string> OfferPrice(OfferPriceModel model, string driverId);
        Task<TripModel> CheckStatus(string driverName);
        Task<IEnumerable<TripDriverModel>> ShowTrips(string driverId);
    }
}
