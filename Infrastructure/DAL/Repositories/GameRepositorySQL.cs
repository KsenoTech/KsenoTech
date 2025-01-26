using Microsoft.EntityFrameworkCore;
using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;

namespace minesweeperAPI.Infrastructure.DAL.Repositories
{
    public class GameRepositorySQL : IRepository<Game>
    {
        private MinesweeperContext _dbcontext;


        public GameRepositorySQL(MinesweeperContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Game> CreateGameRepository(Game game)
        { 

            await _dbcontext.Games.AddAsync(game);
            await _dbcontext.SaveChangesAsync();
            Console.WriteLine($"Game created and saved to DB: {game.GameId}");
            return game;
        }

        public async Task<Game?> GetGameRepository(Guid gameId)
        {
            var game = await _dbcontext.Games
                .FirstOrDefaultAsync(g => g.GameId == gameId);

            Console.WriteLine($"Game retrieved: {gameId} - {(game != null ? "Found" : "Not Found")}");
            return game;
        }

        public async Task UpdateGameRepository(Game game)
        {
            _dbcontext.Games.Update(game);
            await _dbcontext.SaveChangesAsync();
            Console.WriteLine($"Game updated in DB: {game.GameId}");
        }
    }
}
