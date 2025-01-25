using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Services;

namespace minesweeperAPI.Infrastructure.BLL.Services
{
    public class GameService : IGameService
    {
        private readonly MinesweeperContext _context;

        public GameService(MinesweeperContext context)
        {
            _context = context;
        }

        // Создание новой игры
        public async Task<Game> CreateGameAsync(int width, int height, int minesCount)
        {
            if (width <= 0 || height <= 0 || minesCount <= 0 || minesCount >= width * height)
            {
                throw new ArgumentException("Invalid game parameters.");
            }

            var game = new Game
            {
                Width = width,
                Height = height,
                MinesCount = minesCount,
                Completed = false,
                Field = new string[height, width],
                Mines = new bool[height, width]
            };

            InitializeGameField(game);

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return game;
        }

        // Получение игры по ID
        public async Task<Game> GetGameAsync(Guid gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                throw new KeyNotFoundException("Game not found.");
            }

            return game;
        }

        // Выполнение хода
        public async Task<Game> MakeTurnAsync(Guid gameId, int row, int col)
        {
            var game = await GetGameAsync(gameId);

            if (game.Completed)
            {
                throw new InvalidOperationException("Game is already completed.");
            }

            if (row < 0 || row >= game.Height || col < 0 || col >= game.Width)
            {
                throw new ArgumentOutOfRangeException("Invalid cell coordinates.");
            }

            if (game.Field[row, col] != " ")
            {
                throw new InvalidOperationException("Cell is already opened.");
            }

            if (game.Mines[row, col])
            {
                game.Completed = true;
                RevealAllMines(game);
                await _context.SaveChangesAsync();
                return game;
            }

            OpenCell(game, row, col);

            if (IsGameCompleted(game))
            {
                game.Completed = true;
                MarkAllMines(game);
            }

            await _context.SaveChangesAsync();
            return game;
        }

        // Вспомогательные методы
        private void InitializeGameField(Game game)
        {
            var random = new Random();
            int minesPlaced = 0;

            while (minesPlaced < game.MinesCount)
            {
                int row = random.Next(game.Height);
                int col = random.Next(game.Width);

                if (!game.Mines[row, col])
                {
                    game.Mines[row, col] = true;
                    minesPlaced++;
                }
            }

            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    game.Field[row, col] = " ";
                }
            }
        }

        private void OpenCell(Game game, int row, int col)
        {
            if (row < 0 || row >= game.Height || col < 0 || col >= game.Width || game.Field[row, col] != " ")
            {
                return;
            }

            int minesAround = CountMinesAround(game, row, col);

            if (minesAround == 0)
            {
                game.Field[row, col] = "0";

                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr != 0 || dc != 0)
                        {
                            OpenCell(game, row + dr, col + dc);
                        }
                    }
                }
            }
            else
            {
                game.Field[row, col] = minesAround.ToString();
            }
        }

        private int CountMinesAround(Game game, int row, int col)
        {
            int count = 0;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nr = row + dr;
                    int nc = col + dc;

                    if (nr >= 0 && nr < game.Height && nc >= 0 && nc < game.Width && game.Mines[nr, nc])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void RevealAllMines(Game game)
        {
            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    if (game.Mines[row, col])
                    {
                        game.Field[row, col] = "X";
                    }
                }
            }
        }

        private void MarkAllMines(Game game)
        {
            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    if (game.Mines[row, col] && game.Field[row, col] == " ")
                    {
                        game.Field[row, col] = "M";
                    }
                }
            }
        }

        private bool IsGameCompleted(Game game)
        {
            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    if (!game.Mines[row, col] && game.Field[row, col] == " ")
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
