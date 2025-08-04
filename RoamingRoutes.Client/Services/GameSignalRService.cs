using Microsoft.AspNetCore.SignalR.Client;
using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Client.Services;

public interface IGameSignalRService : IAsyncDisposable
{
    // Connection management
    Task StartAsync();
    Task StopAsync();
    bool IsConnected { get; }

    // Game room management
    Task JoinGameRoomAsync(string gameId);
    Task LeaveGameRoomAsync(string gameId);

    // Game actions
    Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request);
    Task<bool> JoinGameAsync(string gameId, JoinGameRequestDTO request);
    Task<bool> StartGameAsync(string gameId);
    Task<bool> SubmitGuessAsync(string gameId, string playerId, string guess);
    Task<bool> CallVoteAsync(string gameId);
    Task<bool> VotePlayerAsync(string gameId, string playerId, string targetPlayerId);

    // Events
    event Func<GameStateDTO, Task>? GameCreated;
    event Func<GameStateDTO, Task>? GameUpdated;
    event Func<GameStateDTO, Task>? PlayerJoined;
    event Func<GameStateDTO, Task>? GameStarted;
    event Func<GuessDTO, Task>? GuessSubmitted;
    event Func<GameStateDTO, Task>? VotingStarted;
    event Func<object, Task>? VoteSubmitted;
}

public class GameSignalRService : IGameSignalRService
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public GameSignalRService(IConfiguration configuration)
    {
        // Get the base URL from configuration or use a default
        var baseUrl = configuration["BaseUrl"] ?? "http://localhost:5216";
        _hubUrl = $"{baseUrl}/gameHub";
    }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    // Events
    public event Func<GameStateDTO, Task>? GameCreated;
    public event Func<GameStateDTO, Task>? GameUpdated;
    public event Func<GameStateDTO, Task>? PlayerJoined;
    public event Func<GameStateDTO, Task>? GameStarted;
    public event Func<GuessDTO, Task>? GuessSubmitted;
    public event Func<GameStateDTO, Task>? VotingStarted;
    public event Func<object, Task>? VoteSubmitted;

    public async Task StartAsync()
    {
        if (_hubConnection is not null)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .Build();

        // Register event handlers
        _hubConnection.On<GameStateDTO>("GameCreated", async (gameState) => 
        {
            if (GameCreated is not null)
                await GameCreated.Invoke(gameState);
        });

        _hubConnection.On<GameStateDTO>("GameUpdated", async (gameState) => 
        {
            if (GameUpdated is not null)
                await GameUpdated.Invoke(gameState);
        });

        _hubConnection.On<GameStateDTO>("PlayerJoined", async (gameState) => 
        {
            if (PlayerJoined is not null)
                await PlayerJoined.Invoke(gameState);
        });

        _hubConnection.On<GameStateDTO>("GameStarted", async (gameState) => 
        {
            if (GameStarted is not null)
                await GameStarted.Invoke(gameState);
        });

        _hubConnection.On<GuessDTO>("GuessSubmitted", async (guess) => 
        {
            if (GuessSubmitted is not null)
                await GuessSubmitted.Invoke(guess);
        });

        _hubConnection.On<GameStateDTO>("VotingStarted", async (gameState) => 
        {
            if (VotingStarted is not null)
                await VotingStarted.Invoke(gameState);
        });

        _hubConnection.On<object>("VoteSubmitted", async (vote) => 
        {
            if (VoteSubmitted is not null)
                await VoteSubmitted.Invoke(vote);
        });

        await _hubConnection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async Task JoinGameRoomAsync(string gameId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            await _hubConnection.SendAsync("JoinGameRoom", gameId);
        }
    }

    public async Task LeaveGameRoomAsync(string gameId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            await _hubConnection.SendAsync("LeaveGameRoom", gameId);
        }
    }

    public async Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<GameStateDTO?>("CreateGame", request);
        }
        return null;
    }

    public async Task<bool> JoinGameAsync(string gameId, JoinGameRequestDTO request)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<bool>("JoinGame", gameId, request);
        }
        return false;
    }

    public async Task<bool> StartGameAsync(string gameId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<bool>("StartGame", gameId);
        }
        return false;
    }

    public async Task<bool> SubmitGuessAsync(string gameId, string playerId, string guess)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<bool>("SubmitGuess", gameId, playerId, guess);
        }
        return false;
    }

    public async Task<bool> CallVoteAsync(string gameId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<bool>("CallVote", gameId);
        }
        return false;
    }

    public async Task<bool> VotePlayerAsync(string gameId, string playerId, string targetPlayerId)
    {
        if (_hubConnection is not null && IsConnected)
        {
            return await _hubConnection.InvokeAsync<bool>("VotePlayer", gameId, playerId, targetPlayerId);
        }
        return false;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
