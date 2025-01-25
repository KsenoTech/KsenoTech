using minesweeperAPI.ApplicationCore.DomModels;

namespace minesweeperAPI.ApplicationCore.Interfaces.Services
{
    public interface IGameService
    {
        Task<Game> CreateGameAsync(int width, int height, int minesCount);
        Task<Game> MakeTurnAsync(Guid gameId, int row, int col);
        Task<Game> GetGameAsync(Guid gameId); // Метод для получения игры по ID
    }

}
