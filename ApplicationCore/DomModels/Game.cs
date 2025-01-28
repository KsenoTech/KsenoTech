using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace minesweeperAPI.ApplicationCore.DomModels
{
    public class Game
    {
        [Key]
        public Guid GameId { get; set; } = Guid.NewGuid();
        public int Width { get; set; }
        public int Height { get; set; }
        public int MinesCount { get; set; }
        public bool Completed { get; set; }



        // Поля для хранения JSON строк
        public string Field { get; set; } // Игровое поле в формате JSON
        public string Mines { get; set; } // Расположение мин в формате JSON

        // Сериализация Field в JSON для хранения в базе
        [NotMapped]
        public List<List<string>> FieldList
        {
            get => JsonSerializer.Deserialize<List<List<string>>>(Field);
            set => Field = JsonSerializer.Serialize(value);
        }

        // Сериализация Mines в JSON для хранения в базе
        [NotMapped]
        public List<List<bool>> MinesList
        {
            get => JsonSerializer.Deserialize<List<List<bool>>>(Mines);
            set => Mines = JsonSerializer.Serialize(value);
        }

    }

    
}
