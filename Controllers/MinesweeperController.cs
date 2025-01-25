using Microsoft.AspNetCore.Mvc;
using minesweeperAPI.ApplicationCore.Interfaces.Services;
using minesweeperAPI.ApplicationCore.Models;

namespace minesweeperAPI.Controllers
{
    [Route("api/[controller]")]
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
            return Ok(game);
        }

        [HttpPost("turn")]
        public async Task<IActionResult> MakeTurn([FromBody] TurnRequest request)
        {
            try
            {
                var game = await _gameService.MakeTurnAsync(request.GameId, request.Row, request.Col);
                return Ok(game);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
