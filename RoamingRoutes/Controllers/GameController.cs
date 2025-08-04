using Microsoft.AspNetCore.Mvc;
using RoamingRoutes.Services;
using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new game
    /// </summary>
    [HttpPost("create")]
    public async Task<ActionResult<GameStateDTO>> CreateGame([FromBody] CreateGameRequestDTO createGame)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createGame.HostNickname))
            {
                return BadRequest("Host nickname is required");
            }

            var gameState = await _gameService.CreateGameAsync(createGame);
            
            _logger.LogInformation("Game created: {GameId} by {CreatorName}", gameState.GameId, createGame.HostNickname);
            
            return Ok(gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game for {CreatorName}", createGame.HostNickname);
            return StatusCode(500, "An error occurred while creating the game");
        }
    }

    /// <summary>
    /// Join an existing game
    /// </summary>
    [HttpPost("{gameId}/join")]
    public async Task<ActionResult<PlayerInfoResponseDTO>> JoinGame(string gameId, [FromBody] JoinGameRequestDTO joinGame)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinGame.Nickname))
            {
                return BadRequest("Player nickname is required");
            }

            if (string.IsNullOrWhiteSpace(gameId))
            {
                return BadRequest("Game ID is required");
            }

            var playerInfo = await _gameService.JoinGameAsync(gameId, joinGame);
            
            _logger.LogInformation("Player {PlayerName} joined game {GameId}", joinGame.Nickname, gameId);
            
            return Ok(playerInfo);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to join game {GameId}: {Error}", gameId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId} for {PlayerName}", gameId, joinGame.Nickname);
            return StatusCode(500, "An error occurred while joining the game");
        }
    }

    /// <summary>
    /// Get current game state
    /// </summary>
    [HttpGet("{gameId}")]
    public async Task<ActionResult<GameStateDTO>> GetGameState(string gameId)
    {
        try
        {
            var gameState = await _gameService.GetGameStateAsync(gameId);
            return Ok(gameState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Game {GameId} not found: {Error}", gameId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game state for {GameId}", gameId);
            return StatusCode(500, "An error occurred while getting the game state");
        }
    }

    /// <summary>
    /// Get list of available games to join
    /// </summary>
    [HttpGet("available")]
    public async Task<ActionResult<List<GameStateDTO>>> GetAvailableGames()
    {
        try
        {
            var games = await _gameService.GetAvailableGamesAsync();
            return Ok(games);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available games");
            return StatusCode(500, "An error occurred while getting available games");
        }
    }

    /// <summary>
    /// Health check endpoint for testing
    /// </summary>
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        return Ok(new { 
            Status = "Healthy", 
            Timestamp = DateTime.UtcNow,
            Service = "Game API"
        });
    }
}
