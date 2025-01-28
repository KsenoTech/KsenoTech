using minesweeperAPI.ApplicationCore.DomModels;
using minesweeperAPI.ApplicationCore.Interfaces.Repositories;
using minesweeperAPI.ApplicationCore.Interfaces.Services;
using System.Text;

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

            var mines = GenerateMines(width, height, minesCount);
            var field = GenerateFieldWithNumbers(width, height, mines);

            var game = new Game
            {
                GameId = Guid.NewGuid(),
                Width = width,
                Height = height,
                MinesCount = minesCount,
                Completed = false,
                FieldList = field,
                VisibleFieldList = GenerateHiddenField(width, height),
                MinesList = mines
            };

            await _db.Games.CreateGameRepository(game);

            return new Game
            {
                GameId = game.GameId,
                Width = width,
                Height = height,
                MinesCount = minesCount,
                Completed = false,
                FieldList = GenerateHiddenField(width, height), // Send hidden field to client initially
                VisibleFieldList = GenerateHiddenField(width, height),
                MinesList = mines
            };
        }
        private List<List<string>> GenerateHiddenField(int width, int height)
        {
            return Enumerable.Range(0, height)
                .Select(_ => Enumerable.Repeat(" ", width).ToList())
                .ToList();
        }

        public async Task<Game> MakeTurnAsync(Guid gameId, int row, int col)
        {
            Console.WriteLine($"MOVE TURN: GameId = {gameId}, Row = {row}, Col = {col}");

            var game = await _db.Games.GetGameRepository(gameId);

            if (game == null)
            {
                Console.WriteLine("Game not found.");
                throw new ArgumentException("Game not found.");
            }

            if (game.Completed)
            {
                Console.WriteLine("Game is already completed.");
                throw new ArgumentException("Game is already completed.");
            }

            // Если пользователь нажал на мину
            if (game.FieldList[row][col] == "X")
            {
                game.Completed = true;
                game.VisibleFieldList[row][col] = "X"; // Показываем мину
                Console.WriteLine($"Mine hit at Row = {row}, Col = {col}. Game over.");
            }
            else
            {
                // Открываем выбранную ячейку и соседние (если пустые)
                OpenCell(game, row, col);
                // Лог текущего состояния видимого поля
                Console.WriteLine($"Visible field after OpenCell:\n{FieldToString(game.VisibleFieldList)}");
            }

            // Проверяем завершение игры
            if (IsGameCompleted(game))
            {
                game.Completed = true;
                Console.WriteLine("Game completed successfully.");
            }

            // Обновляем игру в базе данных
            await _db.Games.UpdateGameRepository(game);

            Console.WriteLine($"Visible Field After Turn:\n{FieldToString(game.VisibleFieldList)}");

            // Подготовка объекта для клиента
            var gameForClient = new Game
            {
                GameId = game.GameId,
                Width = game.Width,
                Height = game.Height,
                MinesCount = game.MinesCount,
                Completed = game.Completed,
                VisibleFieldList = game.VisibleFieldList
            };

            Console.WriteLine($"Updated Field: \n{FieldToString(gameForClient.VisibleFieldList)}");

            return gameForClient;
        }

        private List<List<string>> GetClientField(Game game)
        {
            var fieldForClient = new List<List<string>>();

            for (int row = 0; row < game.Height; row++)
            {
                var rowForClient = new List<string>();
                for (int col = 0; col < game.Width; col++)
                {
                    // Если игра завершена и клетка содержит мину, показываем её
                    if (game.Completed && game.MinesList[row][col])
                    {
                        rowForClient.Add("X");
                    }
                    else
                    {
                        // Иначе показываем только открытые клетки
                        rowForClient.Add(game.VisibleFieldList[row][col] != " " ? game.VisibleFieldList[row][col] : " ");
                    }
                }
                fieldForClient.Add(rowForClient);
            }

            return fieldForClient;
        }



        private void OpenCell(Game game, int row, int col)
        {
            var visibleField = game.VisibleFieldList;
            var field = game.FieldList;

            var queue = new Queue<(int, int)>();
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                var (currentRow, currentCol) = queue.Dequeue();

                if (visibleField[currentRow][currentCol] != " ") // Если клетка уже открыта, пропускаем
                    continue;

                visibleField[currentRow][currentCol] = field[currentRow][currentCol]; // Открываем клетку
                Console.WriteLine($"Opened cell at ({currentRow}, {currentCol}): {field[currentRow][currentCol]}");

                if (field[currentRow][currentCol] == "0") // Если клетка пустая, открываем соседей
                {
                    foreach (var (r, c) in GetNeighbors(game.Width, game.Height, currentRow, currentCol))
                    {
                        if (visibleField[r][c] == " ") // Добавляем только закрытые клетки
                            queue.Enqueue((r, c));
                    }
                }
            }

            // Лог текущего состояния видимого поля
            Console.WriteLine("Visible field after OpenCell:");
            Console.WriteLine(FieldToString(visibleField));
        }




        private List<List<string>> GenerateFieldWithNumbers(int width, int height, List<List<bool>> mines)
        {
            var field = GenerateHiddenField(width, height);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (mines[row][col])
                    {
                        field[row][col] = "X";
                    }
                    else
                    {
                        int count = CountMinesAround(mines, row, col);
                        field[row][col] = count > 0 ? count.ToString() : "0";
                    }
                }
            }

            return field;
        }

        private List<List<bool>> GenerateMines(int width, int height, int minesCount)
        {
            var mines = GenerateHiddenField(width, height)
                .Select(row => row.Select(_ => false).ToList())
                .ToList();

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

            return mines;
        }

        private IEnumerable<(int, int)> GetNeighbors(int width, int height, int row, int col)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;

                    int newRow = row + dr;
                    int newCol = col + dc;

                    if (newRow >= 0 && newRow < height && newCol >= 0 && newCol < width)
                    {
                        yield return (newRow, newCol);
                    }
                }
            }
        }

        private int CountMinesAround(List<List<bool>> mines, int row, int col)
        {
            int count = 0;
            int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dCol = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];

                if (newRow >= 0 && newRow < mines.Count &&
                    newCol >= 0 && newCol < mines[0].Count &&
                    mines[newRow][newCol])
                {
                    count++;
                }
            }

            return count;
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

        private bool IsGameCompleted(Game game)
        {
            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    // Если клетка скрыта и это не мина, игра не завершена
                    if (game.VisibleFieldList[row][col] == " " && game.FieldList[row][col] != "X")
                    {
                        return false;
                    }
                }
            }
            return true; // Все клетки, кроме мин, открыты
        }

    }
}