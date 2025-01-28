using minesweeperAPI.ApplicationCore.DomModels;

namespace minesweeperAPI.ApplicationCore.Interfaces.Services
{
    public interface IGameService
    {

        /// <summary>
        /// Создает новую игру с заданными параметрами.
        /// </summary>
        /// <param name="width">Ширина игрового поля.</param>
        /// <param name="height">Высота игрового поля.</param>
        /// <param name="minesCount">Количество мин.</param>
        /// <returns>Созданная игра.</returns>
        Task<Game> CreateGameAsync(int width, int height, int minesCount);

        /// <summary>
        /// Выполняет ход в игре.
        /// </summary>
        /// <param name="gameId">Идентификатор игры.</param>
        /// <param name="row">Строка хода.</param>
        /// <param name="col">Столбец хода.</param>
        /// <returns>Обновленная игра.</returns>
        Task<Game> MakeTurnAsync(Guid gameId, int row, int col);

    }

}
