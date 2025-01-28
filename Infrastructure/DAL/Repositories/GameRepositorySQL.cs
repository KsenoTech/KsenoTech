using Microsoft.EntityFrameworkCore;
using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;
using System.Text;
using System.Text.Json;

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
            game.VisibleField = JsonSerializer.Serialize(game.VisibleFieldList);
            await _dbcontext.Games.AddAsync(game);
            await _dbcontext.SaveChangesAsync();
            Console.WriteLine($"Game created and saved to DB: {game.GameId}");
            return game;
        }

        public async Task<Game?> GetGameRepository(Guid gameId)
        {
            var game = await _dbcontext.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
            Console.WriteLine($"Game retrieved: {gameId} - {(game != null ? "Found" : "Not Found")}");
            return game;
        }

        public async Task UpdateGameRepository(Game game)
        {
            Console.WriteLine($"Updating game {game.GameId} with field:");
            Console.WriteLine(FieldToString(game.FieldList));
            Console.WriteLine(FieldToString(game.VisibleFieldList));

            var existingGame = await _dbcontext.Games.FirstOrDefaultAsync(g => g.GameId == game.GameId);

            if (existingGame != null)
            {
                existingGame.FieldList = game.FieldList;
                existingGame.VisibleFieldList = game.VisibleFieldList;
                existingGame.MinesList = game.MinesList;
                existingGame.Completed = game.Completed;

                existingGame.Field = JsonSerializer.Serialize(existingGame.FieldList);
                existingGame.Mines = JsonSerializer.Serialize(existingGame.MinesList);
                existingGame.VisibleField = JsonSerializer.Serialize(existingGame.VisibleFieldList);


                // Сохраняем изменения
                await _dbcontext.SaveChangesAsync();
                Console.WriteLine($"Game updated in DB: {game.GameId}");
            }
            else
            {
                throw new InvalidOperationException($"Game with ID {game.GameId} not found for update.");
            }

            
        }

        private string FieldToString(List<List<string>> field)
        {
            if (field == null || field.Count == 0)
            {
                return "Field is empty or null.";
            }
            var builder = new StringBuilder();
            for (int i = 0; i < field.Count; i++)
            {
                if (field[i] == null || field[i].Count == 0)
                {
                    builder.AppendLine($"Row {i} is empty or null.");
                }
                else
                {
                    builder.AppendLine(string.Join(" ", field[i]));
                }
            }
            return builder.ToString();
        }
    }
}
