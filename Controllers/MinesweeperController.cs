using Microsoft.AspNetCore.Mvc;
using minesweeperAPI.ApplicationCore.Interfaces.Services;
using minesweeperAPI.ApplicationCore.Models;

namespace minesweeperAPI.Controllers
{
    [Route("api")]
    //[Route("[controller]")]
    [ApiController]
    public class MinesweeperController : ControllerBase
    {
        private readonly IGameService _gameService;

        public MinesweeperController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("new")]
        public async Task<IActionResult> NewGame([FromBody] NewGameRequest request)
        {
            var game = await _gameService.CreateGameAsync(request.Width, request.Height, request.MinesCount);

            // Преобразуем данные в нужный формат
            var response = new
            {
                game_id = game.GameId,
                width = game.Width,
                height = game.Height,
                mines_count = game.MinesCount,
                field = game.FieldList, // Отправляем объект списка списков
                completed = game.Completed
            };

            return Ok(response);
        }




        [HttpPost("turn")]
        public async Task<IActionResult> MakeTurn([FromBody] TurnRequest request)
        {
            try
            {
                var game = await _gameService.MakeTurnAsync(request.game_id, request.Row, request.Col);

                // Преобразуем данные в нужный формат
                var response = new
                {
                    game_id = game.GameId,
                    width = game.Width,
                    height = game.Height,
                    mines_count = game.MinesCount,
                    field = game.FieldList, // Отправляем объект списка списков
                    completed = game.Completed
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }
}
