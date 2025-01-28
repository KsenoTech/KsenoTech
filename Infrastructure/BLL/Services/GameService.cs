//using System.Text;
//using System.Text.Json;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using minesweeperAPI.ApplicationCore.DomModels;
//using minesweeperAPI.ApplicationCore.Interfaces.Repositories;
//using minesweeperAPI.ApplicationCore.Interfaces.Services;

//namespace minesweeperAPI.Infrastructure.BLL.Services
//{
//    public class GameService : IGameService
//    {
//        private readonly IDbRepository _db;

//        public GameService(IDbRepository db)
//        {
//            _db = db;
//        }

//        public async Task<Game> CreateGameAsync(int width, int height, int minesCount)
//        {
//            if (width > 30 || height > 30 || minesCount >= width * height)
//            {
//                throw new ArgumentException("Invalid game parameters.");
//            }

//            // Генерация мин
//            var mines = GenerateMines(width, height, minesCount);

//            // Генерация игрового поля с числами
//            var field = GenerateFieldWithNumbers(width, height, mines);

//            // Создание игры
//            var game = new Game
//            {
//                GameId = Guid.NewGuid(),
//                Width = width,
//                Height = height,
//                MinesCount = minesCount,
//                Completed = false,
//                FieldList = GenerateFieldWithNumbers(width, height,mines),
//                MinesList = mines
//            };

//            // Сохранение игры в базу данных
//            await _db.Games.CreateGameRepository(game);

//            var gameForClient = new Game
//            {
//                Width = width,
//                Height = height,
//                MinesCount = minesCount,
//                Completed = false,
//                FieldList = GenerateHiddenField(width, height), // Клиент видит закрытое поле
//                MinesList = mines
//            };

//            gameForClient.GameId = game.GameId;

//            return gameForClient;
//        }

//        public async Task<Game> MakeTurnAsync(Guid gameId, int row, int col)
//        {
//            Console.WriteLine($"MOVE TURN: GameId = {gameId}, Row = {row}, Col = {col}");

//            var game = await _db.Games.GetGameRepository(gameId);

//            if (game == null)
//            {
//                Console.WriteLine("Game not found.");
//                throw new ArgumentException("Game not found.");
//            }

//            if (game.Completed)
//            {
//                Console.WriteLine("Game is already completed.");
//                throw new ArgumentException("Game is already completed.");
//            }

//            // Если клетка содержит мину, завершаем игру
//            if (game.MinesList[row][col])
//            {
//                game.Completed = true;
//                game.FieldList[row][col] = "X"; // Обозначаем мину
//                Console.WriteLine($"Mine hit at Row = {row}, Col = {col}. Game over.");
//            }
//            else
//            {
//                // Открываем клетку и рекурсивно открываем соседние, если нужно
//                OpenCell(game, row, col);
//            }

//            // Проверяем, завершена ли игра
//            if (IsGameCompleted(game))
//            {
//                game.Completed = true;
//                Console.WriteLine("Game completed successfully.");
//            }

//            // Создаем объект для клиента, чтобы передать только открытые клетки и скрытые клетки как пробелы
//            var gameForClient = new Game
//            {
//                GameId = game.GameId,
//                Width = game.Width,
//                Height = game.Height,
//                MinesCount = game.MinesCount,
//                Completed = game.Completed,
//                FieldList = GetClientField(game) // Отправляем только обновленное поле
//            };

//            Console.WriteLine($"Updated Field: \n{FieldToString(gameForClient.FieldList)}");

//            return gameForClient;
//        }

//        private List<List<string>> GetClientField(Game game)
//        {
//            var fieldForClient = new List<List<string>>();

//            for (int row = 0; row < game.Height; row++)
//            {
//                var rowForClient = new List<string>();
//                for (int col = 0; col < game.Width; col++)
//                {
//                    // Если клетка была открыта, показываем ее значение, иначе пробел
//                    if (game.FieldList[row][col] != " " && game.FieldList[row][col] != "0")
//                    {
//                        rowForClient.Add(game.FieldList[row][col]); // Значение клетки
//                    }
//                    else
//                    {
//                        rowForClient.Add(" "); // Клетка скрыта
//                    }
//                }
//                fieldForClient.Add(rowForClient);
//            }

//            return fieldForClient;
//        }

//        private void OpenCell(Game game, int row, int col)
//        {
//            // Если клетка уже открыта или не существует, выходим
//            if (game.FieldList[row][col] != " " || game.MinesList[row][col]) return;

//            // Открываем клетку
//            game.FieldList[row][col] = GetAdjacentMinesCount(game, row, col).ToString();

//            // Если клетка не имеет мин рядом, рекурсивно открываем соседние клетки
//            if (game.FieldList[row][col] == "0")
//            {
//                for (int i = -1; i <= 1; i++)
//                {
//                    for (int j = -1; j <= 1; j++)
//                    {
//                        int newRow = row + i;
//                        int newCol = col + j;
//                        if (newRow >= 0 && newRow < game.Height && newCol >= 0 && newCol < game.Width)
//                        {
//                            OpenCell(game, newRow, newCol);
//                        }
//                    }
//                }
//            }
//        }

//        private int GetAdjacentMinesCount(Game game, int row, int col)
//        {
//            int count = 0;
//            for (int i = -1; i <= 1; i++)
//            {
//                for (int j = -1; j <= 1; j++)
//                {
//                    int newRow = row + i;
//                    int newCol = col + j;
//                    if (newRow >= 0 && newRow < game.Height && newCol >= 0 && newCol < game.Width)
//                    {
//                        if (game.MinesList[newRow][newCol]) count++;
//                    }
//                }
//            }
//            return count;
//        }



//        //private void OpenCell(Game game, int row, int col, HashSet<(int, int)> visited = null)
//        //{
//        //    var field = game.FieldList;
//        //    var mines = game.MinesList;



//        //    if (visited == null)
//        //    {
//        //        visited = new HashSet<(int, int)>();
//        //    }

//        //    if (visited.Contains((row, col)) || mines[row][col])
//        //    {
//        //        return;
//        //    }

//        //    visited.Add((row, col));

//        //    // Подсчитываем количество мин вокруг клетки
//        //    int minesAround = CountMinesAround(mines, row, col);
//        //    if (minesAround > 0)
//        //    {
//        //        field[row][col] = minesAround.ToString(); // Записываем количество мин в клетку
//        //        Console.WriteLine($"Cell [{row}, {col}] updated to {minesAround}.");
//        //    }
//        //    else
//        //    {
//        //        field[row][col] = "0"; // Обозначаем, что клетка открыта
//        //        Console.WriteLine($"Cell [{row}, {col}] opened and set to 0.");

//        //        // Рекурсивно открываем соседние клетки
//        //        foreach (var (r, c) in GetNeighbors(game.Width, game.Height, row, col))
//        //        {
//        //            OpenCell(game, r, c, visited);
//        //        }
//        //    }

//        //    // Логирование текущего состояния поля
//        //    Console.WriteLine($"Current Field after opening cell [{row}, {col}]:");
//        //    Console.WriteLine(FieldToString(field));
//        //}
//        //-------------------------------------------------------------------------------------------------------


//        private List<List<string>> GenerateHiddenField(int width, int height)
//        {
//            return Enumerable.Range(0, height)
//                .Select(_ => Enumerable.Repeat(" ", width).ToList())
//                .ToList();
//        }

//        private List<List<string>> GenerateFieldWithNumbers(int width, int height, List<List<bool>> mines)
//        {
//            var field = GenerateHiddenField(width, height);

//            for (int row = 0; row < height; row++)
//            {
//                for (int col = 0; col < width; col++)
//                {
//                    if (mines[row][col])
//                    {
//                        field[row][col] = "X";
//                    }
//                    else
//                    {
//                        int count = CountMinesAround(mines, row, col);
//                        field[row][col] = count > 0 ? count.ToString() : "0";
//                    }
//                }
//            }

//            return field;
//        }

//        private List<List<bool>> GenerateMines(int width, int height, int minesCount)
//        {
//            var mines = GenerateHiddenField(width, height)
//                .Select(row => row.Select(_ => false).ToList())
//                .ToList();

//            var random = new Random();
//            int placedMines = 0;

//            while (placedMines < minesCount)
//            {
//                int row = random.Next(height);
//                int col = random.Next(width);

//                if (!mines[row][col])
//                {
//                    mines[row][col] = true;
//                    placedMines++;
//                }
//            }

//            return mines;
//        }

//        //private void OpenCell(Game game, int row, int col, HashSet<(int, int)> visited = null)
//        //{
//        //    var field = game.FieldList;
//        //    var mines = game.MinesList;

//        //    if (visited == null)
//        //    {
//        //        visited = new HashSet<(int, int)>();
//        //    }

//        //    if (visited.Contains((row, col)))
//        //    {
//        //        return;
//        //    }

//        //    visited.Add((row, col));

//        //    if (mines[row][col])
//        //    {
//        //        return;
//        //    }

//        //    // Подсчитываем количество мин вокруг клетки
//        //    int minesAround = CountMinesAround(mines, row, col);
//        //    if (minesAround > 0)
//        //    {
//        //        field[row][col] = minesAround.ToString(); // Записываем количество мин в клетку
//        //        Console.WriteLine($"Cell [{row}, {col}] updated to {minesAround}.");
//        //    }
//        //    else
//        //    {
//        //        field[row][col] = "0"; // Обозначаем, что клетка открыта
//        //        Console.WriteLine($"Cell [{row}, {col}] opened and set to 0.");

//        //        // Рекурсивно открываем соседние клетки
//        //        foreach (var (r, c) in GetNeighbors(game.Width, game.Height, row, col))
//        //        {
//        //            OpenCell(game, r, c, visited);
//        //        }
//        //    }

//        //    // Логирование текущего состояния поля
//        //    Console.WriteLine($"Current Field after opening cell [{row}, {col}]:");
//        //    Console.WriteLine(FieldToString(field));
//        //}
//        private string FieldToString(List<List<string>> field)
//        {
//            if (field == null || field.Count == 0)
//            {
//                return "Field is empty or null.";
//            }
//            var builder = new StringBuilder();
//            for (int i = 0; i < field.Count; i++)
//            {
//                if (field[i] == null || field[i].Count == 0)
//                {
//                    builder.AppendLine($"Row {i} is empty or null.");
//                }
//                else
//                {
//                    builder.AppendLine(string.Join(" ", field[i]));
//                }
//            }
//            return builder.ToString();
//        }

//        private int CountMinesAround(List<List<bool>> mines, int row, int col)
//        {
//            int count = 0;
//            int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 };
//            int[] dCol = { -1, 0, 1, -1, 1, -1, 0, 1 };

//            for (int i = 0; i < 8; i++)
//            {
//                int newRow = row + dRow[i];
//                int newCol = col + dCol[i];

//                if (newRow >= 0 && newRow < mines.Count &&
//                    newCol >= 0 && newCol < mines[0].Count &&
//                    mines[newRow][newCol])
//                {
//                    count++;
//                }
//            }

//            return count;
//        }

//        private IEnumerable<(int, int)> GetNeighbors(int width, int height, int row, int col)
//        {
//            for (int dr = -1; dr <= 1; dr++)
//            {
//                for (int dc = -1; dc <= 1; dc++)
//                {
//                    if (dr == 0 && dc == 0) continue;

//                    int newRow = row + dr;
//                    int newCol = col + dc;

//                    if (newRow >= 0 && newRow < height && newCol >= 0 && newCol < width)
//                    {
//                        yield return (newRow, newCol);
//                    }
//                }
//            }
//        }

//        private bool IsGameCompleted(Game game)
//        {
//            // Проверяем, если все клетки открыты и нет мин
//            foreach (var row in game.FieldList)
//            {
//                foreach (var cell in row)
//                {
//                    if (cell == " " || cell == "0") // Если клетка скрыта
//                    {
//                        return false; // Игра не завершена
//                    }
//                }
//            }
//            return true; // Все клетки открыты
//        }


//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

            var mines = GenerateMines(width, height, minesCount);
            var field = GenerateFieldWithNumbers(width, height, mines);

            var game = new Game
            {
                GameId = Guid.NewGuid(),
                Width = width,
                Height = height,
                MinesCount = minesCount,
                Completed = false,
                FieldList = field,//GenerateHiddenField(width, height), // Initial field is hidden
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
                MinesList = mines
            };
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

            // Если клетка содержит мину, завершаем игру
            if (game.MinesList[row][col])
            {
                game.Completed = true;
                game.FieldList[row][col] = "X"; // Обозначаем мину
                Console.WriteLine($"Mine hit at Row = {row}, Col = {col}. Game over.");
            }
            else
            {
                // Открываем клетку и рекурсивно открываем соседние, если нужно
                OpenCell(game, row, col);
            }

            // Проверяем, завершена ли игра
            if (IsGameCompleted(game))
            {
                game.Completed = true;
                Console.WriteLine("Game completed successfully.");
            }

            // Создаем объект для клиента, чтобы передать только открытые клетки и скрытые клетки как пробелы
            var gameForClient = new Game
            {
                GameId = game.GameId,
                Width = game.Width,
                Height = game.Height,
                MinesCount = game.MinesCount,
                Completed = game.Completed,
                FieldList = GetClientField(game) // Отправляем только обновленное поле
            };

            Console.WriteLine($"Updated Field: \n{FieldToString(gameForClient.FieldList)}");

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
                    // Show the value if the cell is opened, otherwise show a space
                    rowForClient.Add(game.FieldList[row][col] != " " ? game.FieldList[row][col] : " ");
                }
                fieldForClient.Add(rowForClient);
            }

            return fieldForClient;
        }

        private void OpenCell(Game game, int row, int col)
        {
            var field = game.FieldList;
            var mines = game.MinesList;

            // Создаем очередь для клеток, которые нужно открыть
            var queue = new Queue<(int, int)>();
            queue.Enqueue((row, col));

            while (queue.Count > 0)
            {
                var (currentRow, currentCol) = queue.Dequeue();

                // Если клетка уже открыта или это мина, пропускаем
                if (field[currentRow][currentCol] != " " || mines[currentRow][currentCol])
                    continue;

                // Подсчитываем количество мин вокруг клетки
                int minesAround = CountMinesAround(mines, currentRow, currentCol);
                if (minesAround > 0)
                {
                    field[currentRow][currentCol] = minesAround.ToString(); // Записываем количество мин в клетку
                }
                else
                {
                    field[currentRow][currentCol] = "0"; // Обозначаем, что клетка открыта
                                                         // Если клетка не имеет мин рядом, добавляем соседей в очередь
                    foreach (var (r, c) in GetNeighbors(game.Width, game.Height, currentRow, currentCol))
                    {
                        queue.Enqueue((r, c));
                    }
                }

                // Логируем состояние поля
                Console.WriteLine($"Current Field after opening cell [{currentRow}, {currentCol}]:");
                Console.WriteLine(FieldToString(field));
            }
        }

        private List<List<string>> GenerateHiddenField(int width, int height)
        {
            return Enumerable.Range(0, height)
                .Select(_ => Enumerable.Repeat(" ", width).ToList())
                .ToList();
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
            // Проверяем, если все клетки открыты и нет мин
            foreach (var row in game.FieldList)
            {
                foreach (var cell in row)
                {
                    if (cell == " " || cell == "0") // Если клетка скрыта
                    {
                        return false; // Игра не завершена
                    }
                }
            }
            return true; // Все клетки открыты
        }

    }
}
