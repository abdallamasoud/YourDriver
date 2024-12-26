using System;
using YourDriver.Model.Db;
using YourDriver.repository.Interfaces;

namespace YourDriver.repository.Classess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly YourDriverDContext _Context;

        public IDriverRepo drivers { get; private set; }
        public IPassengerRepo passengers { get; private set; }

        public UnitOfWork(YourDriverDContext context)
        {
          
            this.drivers = new DriverRepo(context);
            this.passengers = new PassengerRepo(context);
            _Context = context;
        }

        public int complete()
        {
            return _Context.SaveChanges();
        }

        public void Dispose()
        {
            _Context.Dispose();
        }
    }
}
