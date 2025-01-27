namespace minesweeperAPI.ApplicationCore.Models
{
    public class TurnRequest
    {
        public Guid game_id { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
}
