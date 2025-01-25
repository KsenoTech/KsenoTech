using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;

namespace minesweeperAPI.Infrastructure.DAL.Repositories
{
    public class GameRepositorySQL : IRepository<Game>
    {
        private MinesweeperContext _dbcontext;

        private readonly Dictionary<Guid, Game> _games = new();

        public GameRepositorySQL(MinesweeperContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public Task<Game> CreateGameRepository(Game game)
        {
            _games[game.GameId] = game;
            return Task.FromResult(game);
        }

        public Task<Game?> GetGameRepository(Guid gameId)
        {
            _games.TryGetValue(gameId, out var game);
            return Task.FromResult(game);
        }

        public Task UpdateGameRepository(Game game)
        {
            if (_games.ContainsKey(game.GameId))
            {
                _games[game.GameId] = game;
            }
            return Task.CompletedTask;
        }
    }
}
