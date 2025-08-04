using RoamingRoutes.Shared.Models.Games;
using System.Collections.Concurrent;

namespace RoamingRoutes.Services;

public interface IGameService
{
    Task<GameStateDTO> CreateGameAsync(CreateGameRequestDTO createGame);
    Task<PlayerInfoResponseDTO> JoinGameAsync(string gameId, JoinGameRequestDTO joinGame);
    Task<GameStateDTO> GetGameStateAsync(string gameId);
    Task<List<GameStateDTO>> GetAvailableGamesAsync();
}

public class GameService : IGameService
{
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private readonly ILogger<GameService> _logger;

    public GameService(ILogger<GameService> logger)
    {
        _logger = logger;
    }

    public async Task<GameStateDTO> CreateGameAsync(CreateGameRequestDTO createGame)
    {
        var gameId = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid().ToString();
        
        var player = new Player
        {
            Id = playerId,
            Name = createGame.HostNickname,
            IsHost = true,
            JoinedAt = DateTime.UtcNow
        };

        var game = new Game
        {
            Id = gameId,
            HostPlayerId = playerId,
            MaxPlayers = GameConstants.GameConfig.MaxPlayers,
            CreatedAt = DateTime.UtcNow,
            Status = GameStatus.WaitingForPlayers,
            Players = new List<Player> { player }
        };

        _games[gameId] = game;

        _logger.LogInformation("Game {GameId} created by {PlayerName}", gameId, createGame.HostNickname);

        return MapToGameStateDTO(game);
    }

    public async Task<PlayerInfoResponseDTO> JoinGameAsync(string gameId, JoinGameRequestDTO joinGame)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        if (game.Status != GameStatus.WaitingForPlayers)
        {
            throw new InvalidOperationException("Game is not accepting new players");
        }

        if (game.Players.Count >= game.MaxPlayers)
        {
            throw new InvalidOperationException("Game is full");
        }

        if (game.Players.Any(p => p.Name.Equals(joinGame.Nickname, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Player name already taken");
        }

        var playerId = Guid.NewGuid().ToString();
        var player = new Player
        {
            Id = playerId,
            Name = joinGame.Nickname,
            IsHost = false,
            JoinedAt = DateTime.UtcNow
        };

        game.Players.Add(player);

        _logger.LogInformation("Player {PlayerName} joined game {GameId}", joinGame.Nickname, gameId);

        return new PlayerInfoResponseDTO
        {
            GameState = MapToGameStateDTO(game)
        };
    }

    public async Task<GameStateDTO> GetGameStateAsync(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        return MapToGameStateDTO(game);
    }

    public async Task<List<GameStateDTO>> GetAvailableGamesAsync()
    {
        return _games.Values
            .Where(g => g.Status == GameStatus.WaitingForPlayers && g.Players.Count < g.MaxPlayers)
            .Select(MapToGameStateDTO)
            .OrderByDescending(g => g.CreatedAt)
            .ToList();
    }

    private GameStateDTO MapToGameStateDTO(Game game)
    {
        return new GameStateDTO
        {
            GameId = game.Id,
            HostPlayerId = game.HostPlayerId,
            Status = game.Status,
            Players = game.Players.Select(p => new PlayerDTO
            {
                Id = p.Id,
                Nickname = p.Name,
                IsAlive = !p.IsEliminated,
                IsHost = p.IsHost
            }).ToList(),
            CurrentRound = game.CurrentRound,
            MaxPlayers = game.MaxPlayers,
            CreatedAt = game.CreatedAt,
            StartedAt = game.StartedAt,
            VotingPhase = game.VotingPhase,
            RoundStartTime = game.RoundStartTime,
            RecentGuesses = new List<GuessDTO>()
        };
    }
}

// Internal models for in-memory storage
internal class Game
{
    public string Id { get; set; } = string.Empty;
    public string HostPlayerId { get; set; } = string.Empty;
    public GameStatus Status { get; set; }
    public List<Player> Players { get; set; } = new();
    public int CurrentRound { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime RoundStartTime { get; set; }
    public bool VotingPhase { get; set; }
}

internal class Player
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsEliminated { get; set; }
    public bool IsHost { get; set; }
    public DateTime JoinedAt { get; set; }
}
