namespace YourDriver.repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDriverRepo drivers { get; }
        IPassengerRepo passengers { get; }

        int complete();
    }
}
