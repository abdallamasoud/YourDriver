using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourDriver.Model.DriversData;
using YourDriver.Model.NotificationsAndOffers;
using YourDriver.Model.PassengersData;
using YourDriver.Model.Tripes;

namespace YourDriver.Model.Db
{
    public class YourDriverDContext : DbContext
    {
        public YourDriverDContext(DbContextOptions<YourDriverDContext> options ):base(options) 
        {
            
        }


        public DbSet<Driver> drivers { get; set; }
        public DbSet<Passenger> passengers { get; set; }
        public DbSet<Trip> trips { get; set; }
        public DbSet<NotificationModel> notifications { get; set; }
    }
}
