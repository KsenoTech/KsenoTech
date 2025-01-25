namespace minesweeperAPI.ApplicationCore.DomModels
{
    public class Game
    {
        public Guid GameId { get; set; } = Guid.NewGuid();
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinesCount { get; set; }
        public bool Completed { get; set; }
        public string[,] Field { get; set; } // Игровое поле
        public bool[,] Mines { get; set; }  // Расположение мин
    }
}
