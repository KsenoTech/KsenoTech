using minesweeperAPI.ApplicationCore.DomModels;

namespace minesweeperAPI.ApplicationCore.Interfaces.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> CreateGameRepository(T game);
        Task<T?> GetGameRepository(Guid gameId);
        Task UpdateGameRepository(T game);
    }
}
