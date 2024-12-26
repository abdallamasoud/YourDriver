using Microsoft.EntityFrameworkCore;
using System;
using YourDriver.Model.Db;
using YourDriver.Model.DriversData;
using YourDriver.Model.PassengersData;
using YourDriver.repository.Interfaces;

namespace YourDriver.TripServices
{

    public class TripService : ITripService
    {
        private readonly IDriverRepo driverRepo;
        private readonly YourDriverDContext _Context;
        private List<Driver> driversList;

        public TripService(IDriverRepo driverRepo, YourDriverDContext context)
        {
            this.driverRepo = driverRepo;
            _Context = context;
            driversList = context.drivers.ToList();
        }



        public async Task AddAsync(DriverRegister newDriver)
        {
            var driverDb = await _Context.drivers.SingleOrDefaultAsync(d => d.UserName.Equals(newDriver.UserName));
            driversList.Add(driverDb);
            var size = driversList.Count;
            Console.WriteLine(size);
        }

        public async Task DeleteAsync(DriverRegister driver)
        {
            var driverDb = await _Context.drivers.SingleOrDefaultAsync(d => d.UserName == driver.UserName);
            driversList.Remove(driverDb);
        }

        public async Task NotifyAsync(string Location, string Destination, Passenger passenger)
        {
            foreach (var driver in driversList)
            {
                foreach (var area in driver.FavAreas)
                {
                    if (area.ToLower().Equals(Location.ToLower()))
                    {

                        await driverRepo.UpdateNotificationAsync(driver, Location, Destination, passenger);
                    }
                }
            }
            _Context.SaveChanges();
        }

        public async Task requestTrip(string Location, string Destination, string Id)
        {
            var passenger = await _Context.passengers.SingleAsync(p => p.AppUserId == Id);
            await NotifyAsync(Location, Destination, passenger);
        }

    }
}
