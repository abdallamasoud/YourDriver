using YourDriver.Model.DriversData;
using YourDriver.Model.NotificationsAndOffers;
using YourDriver.Model.PassengersData;
using YourDriver.Model.Tripes;

namespace YourDriver.repository.Interfaces
{
    public interface IPassengerRepo : IBaseRepo<Passenger>
    {
        Task<PassengerUpdateModel> UpdatePassenger(PassengerUpdateModel newDriPassenger, string passengerId);
        Task<List<ShowOffersModel>> ShowOffers(string name);
        Task<DriverInfoModel> ChooseOffer(int offerNumber, string Id);
        Task<List<TripPassengerModel>> ShowTrips(string PassengerId);
        Task<bool> RateDriver(string name, int rating);

    }
}
