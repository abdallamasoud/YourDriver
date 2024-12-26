using System.ComponentModel.DataAnnotations.Schema;
using YourDriver.Model.NotificationsAndOffers;
using YourDriver.Model.Tripes;

namespace YourDriver.Model.DriversData
 

{
    public class Driver
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public byte[]? CarPhoto { get; set; }
        public string? AppUserId { get; set; }
        public string LicenseNumber { get; set; }
        [NotMapped]
        public List<string> FavAreas { get; set; }
        public List<NotificationModel>? Noitfications { get; set; }
        public List<Trip> Trips { get; set; }
        public double? Rating { get; set; }
        public bool IsLogged { get; set; }

    }
} 
