using Microsoft.AspNetCore.Mvc;
using minesweeperAPI.ApplicationCore.DomModels;
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
            var game = await _gameService.CreateGameAsync(request.width, request.height, request.mines_count);

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
                // Выполняем ход
                var updatedGame = await _gameService.MakeTurnAsync(request.game_id, request.Row, request.Col);

                // Возвращаем обновленное состояние игры
                return Ok(new
                {
                    game_id = updatedGame.GameId,
                    width = updatedGame.Width,
                    height = updatedGame.Height,
                    mines_count = updatedGame.MinesCount,
                    field = updatedGame.FieldList, // Поле с обновленным состоянием
                    completed = updatedGame.Completed
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", details = ex.Message });
            }
        }


    }
}
