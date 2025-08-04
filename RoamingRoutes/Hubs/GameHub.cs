using Microsoft.AspNetCore.SignalR;
using RoamingRoutes.Services;
using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Hubs;

public class GameHub : Hub
{
    private readonly IGameService _gameService;
    private readonly ILogger<GameHub> _logger;

    public GameHub(IGameService gameService, ILogger<GameHub> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    /// <summary>
    /// Join a specific game room for real-time updates
    /// </summary>
    public async Task JoinGameRoom(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"game-{gameId}");
        _logger.LogInformation("Connection {ConnectionId} joined game room {GameId}", Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Leave a specific game room
    /// </summary>
    public async Task LeaveGameRoom(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game-{gameId}");
        _logger.LogInformation("Connection {ConnectionId} left game room {GameId}", Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Create a new game with real-time notification
    /// </summary>
    public async Task<GameStateDTO?> CreateGame(CreateGameRequestDTO request)
    {
        try
        {
            var gameState = await _gameService.CreateGameAsync(request);
            if (gameState != null)
            {
                // Join the creator to the game room
                await JoinGameRoom(gameState.GameId);
                
                // Notify all clients about the new available game
                await Clients.All.SendAsync("GameCreated", gameState);
                
                _logger.LogInformation("Game {GameId} created by connection {ConnectionId}", gameState.GameId, Context.ConnectionId);
            }
            return gameState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game from connection {ConnectionId}", Context.ConnectionId);
            return null;
        }
    }

    /// <summary>
    /// Join a game with real-time notification
    /// </summary>
    public async Task<bool> JoinGame(string gameId, JoinGameRequestDTO request)
    {
        try
        {
            var joinResult = await _gameService.JoinGameAsync(gameId, request);
            if (joinResult != null)
            {
                // Join the player to the game room
                await JoinGameRoom(gameId);
                
                // Get updated game state
                var gameState = await _gameService.GetGameStateAsync(gameId);
                if (gameState != null)
                {
                    // Notify all players in the game about the new player
                    await Clients.Group($"game-{gameId}").SendAsync("PlayerJoined", gameState);
                    
                    // Update the available games list for lobby users
                    await Clients.All.SendAsync("GameUpdated", gameState);
                }
                
                _logger.LogInformation("Player joined game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
            return false;
        }
    }

    /// <summary>
    /// Start a game with real-time notification
    /// </summary>
    public async Task<bool> StartGame(string gameId)
    {
        try
        {
            // This would update the game status to InProgress
            // For now, we'll just notify the game room
            var gameState = await _gameService.GetGameStateAsync(gameId);
            if (gameState != null)
            {
                // In a real implementation, you'd update the game status here
                // gameState.Status = GameStatus.InProgress;
                
                await Clients.Group($"game-{gameId}").SendAsync("GameStarted", gameState);
                _logger.LogInformation("Game {GameId} started by connection {ConnectionId}", gameId, Context.ConnectionId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
            return false;
        }
    }

    /// <summary>
    /// Submit a guess with real-time notification
    /// </summary>
    public async Task<bool> SubmitGuess(string gameId, string playerId, string guess)
    {
        try
        {
            // Create a guess DTO
            var guessDto = new GuessDTO
            {
                PlayerId = playerId,
                GuessedWord = guess,
                Timestamp = DateTime.UtcNow,
                PlayerName = "Player", // In a real app, you'd get this from the player info
            };

            // In a real implementation, you'd save this guess to the game state
            // For now, we'll just broadcast it
            await Clients.Group($"game-{gameId}").SendAsync("GuessSubmitted", guessDto);
            
            _logger.LogInformation("Guess submitted for game {GameId} by connection {ConnectionId}", gameId, Context.ConnectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting guess for game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
            return false;
        }
    }

    /// <summary>
    /// Call for vote with real-time notification
    /// </summary>
    public async Task<bool> CallVote(string gameId)
    {
        try
        {
            var gameState = await _gameService.GetGameStateAsync(gameId);
            if (gameState != null)
            {
                // In a real implementation, you'd update the game to voting phase
                // gameState.VotingPhase = true;
                
                await Clients.Group($"game-{gameId}").SendAsync("VotingStarted", gameState);
                _logger.LogInformation("Voting called for game {GameId} by connection {ConnectionId}", gameId, Context.ConnectionId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling vote for game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
            return false;
        }
    }

    /// <summary>
    /// Submit a vote to eliminate a player
    /// </summary>
    public async Task<bool> VotePlayer(string gameId, string playerId, string targetPlayerId)
    {
        try
        {
            // In a real implementation, you'd record the vote and check if voting is complete
            await Clients.Group($"game-{gameId}").SendAsync("VoteSubmitted", new { PlayerId = playerId, TargetPlayerId = targetPlayerId });
            
            _logger.LogInformation("Vote submitted for game {GameId} by connection {ConnectionId}", gameId, Context.ConnectionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting vote for game {GameId} from connection {ConnectionId}", gameId, Context.ConnectionId);
            return false;
        }
    }

    /// <summary>
    /// Handle client disconnection
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Connection {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
