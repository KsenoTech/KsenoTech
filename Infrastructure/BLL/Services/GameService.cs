using System.Text.Json;
using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;
using minesweeperAPI.ApplicationCore.Interfaces.Services;

namespace minesweeperAPI.Infrastructure.BLL.Services
{
    public class GameService : IGameService
    {
        private readonly IDbRepository _db;

        public GameService(IDbRepository db)
        {
            _db = db;
        }

        public async Task<Game> CreateGameAsync(int width, int height, int minesCount)
        {
            if (width > 30 || height > 30 || minesCount >= width * height)
            {
                throw new ArgumentException("Invalid game parameters.");
            }

            var field = GenerateField(width, height);
            var mines = GenerateMines(width, height, minesCount);

            var game = new Game
            {
                Width = width,
                Height = height,
                MinesCount = minesCount,
                FieldList = field,
                MinesList = mines
            };

            var gameq = await _db.Games.CreateGameRepository(game);
            if (gameq == null)
            {
                Console.WriteLine("Игра не сохораниалсь");
            }
            // Сохраняем игру в репозитории
            return game;
        }

        public async Task<Game> MakeTurnAsync(Guid gameId, int row, int col)
        {
            var game = await _db.Games.GetGameRepository(gameId);
            Console.WriteLine($"MakeTurnAsync {gameId}");
            if (game == null)
            {
                throw new ArgumentException("Game not found.");
            }

            if (game.Completed)
            {
                throw new ArgumentException("Game is already completed.");
            }

            var field = game.FieldList;
            var mines = game.MinesList;

            if (field[row][col] != " ")
            {
                throw new ArgumentException("Cell already opened.");
            }

            if (mines[row][col])
            {
                // Игрок попал на мину
                field[row][col] = "X";
                game.Completed = true;
                RevealAllCells(game);
            }
            else
            {
                // Открываем клетку и соседние
                OpenCell(game, row, col);
            }

            await _db.Games.UpdateGameRepository(game); // Обновляем игру в репозитории
            return game;
        }

        public async Task<Game> GetGameAsync(Guid gameId)
        {
            var game = await _db.Games.GetGameRepository(gameId);
            if (game == null)
            {
                throw new ArgumentException("Game not found.");
            }

            return game;
        }

        // Вспомогательные методы
        private List<List<string>> GenerateField(int width, int height)
        {
            var field = new List<List<string>>();
            for (int i = 0; i < height; i++)
            {
                field.Add(Enumerable.Repeat(" ", width).ToList());
            }
            return field;
        }

        private List<List<bool>> GenerateMines(int width, int height, int minesCount)
        {
            var mines = new List<List<bool>>();
            for (int i = 0; i < height; i++)
            {
                mines.Add(Enumerable.Repeat(false, width).ToList());
            }

            var random = new Random();
            int placedMines = 0;

            while (placedMines < minesCount)
            {
                int row = random.Next(height);
                int col = random.Next(width);

                if (!mines[row][col])
                {
                    mines[row][col] = true;
                    placedMines++;
                }
            }

            return mines; // Возвращаем List<List<bool>> вместо строки
        }


        private void OpenCell(Game game, int row, int col)
        {
            var field = game.FieldList;
            var mines = game.MinesList;

            // Проверяем, не вышли ли мы за границы поля
            if (row < 0 || col < 0 || row >= game.Height || col >= game.Width || field[row][col] != " ")
            {
                return;
            }

            var queue = new Queue<(int, int)>();
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                var (currentRow, currentCol) = queue.Dequeue();

                if (field[currentRow][currentCol] != " ") continue;

                var adjacentMines = CountAdjacentMines(mines, currentRow, currentCol);
                field[currentRow][currentCol] = adjacentMines > 0 ? adjacentMines.ToString() : "0";

                // Если соседей с миной нет, добавляем соседей в очередь
                if (adjacentMines == 0)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            int newRow = currentRow + i;
                            int newCol = currentCol + j;

                            // Проверяем, не вышли ли мы за границы поля
                            if (newRow >= 0 && newCol >= 0 && newRow < game.Height && newCol < game.Width)
                            {
                                queue.Enqueue((newRow, newCol));
                            }
                        }
                    }
                }
            }
        }


        private int CountAdjacentMines(List<List<bool>> mines, int row, int col)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;

                    int newRow = row + i;
                    int newCol = col + j;

                    if (newRow >= 0 && newCol >= 0 && newRow < mines.Count && newCol < mines[0].Count)
                    {
                        if (mines[newRow][newCol])
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private void RevealAllCells(Game game)
        {
            var field = game.FieldList;
            var mines = game.MinesList;

            for (int i = 0; i < game.Height; i++)
            {
                for (int j = 0; j < game.Width; j++)
                {
                    if (mines[i][j])
                    {
                        field[i][j] = "M";
                    }
                    else if (field[i][j] == " ")
                    {
                        field[i][j] = CountAdjacentMines(mines, i, j).ToString();
                    }
                }
            }
        }
    }
}
