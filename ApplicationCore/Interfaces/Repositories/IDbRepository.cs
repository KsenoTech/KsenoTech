using minesweeperAPI.ApplicationCore.DomModels;

namespace minesweeperAPI.ApplicationCore.Interfaces.Repositories
{
    public interface IDbRepository
    {
        IRepository<Game> Games { get; }
        Task SaveChangesAsync();
    }
}
