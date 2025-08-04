using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Client.Services;

/// <summary>
/// Server-side implementation of SignalR service (does nothing since SignalR runs on server)
/// </summary>
public class ServerSideGameSignalRService : IGameSignalRService
{
    public bool IsConnected => true; // Always connected on server-side

    public event Func<GameStateDTO, Task>? GameCreated;
    public event Func<GameStateDTO, Task>? GameUpdated;
    public event Func<GameStateDTO, Task>? PlayerJoined;
    public event Func<GameStateDTO, Task>? GameStarted;
    public event Func<GuessDTO, Task>? GuessSubmitted;
    public event Func<GameStateDTO, Task>? VotingStarted;
    public event Func<object, Task>? VoteSubmitted;

    public Task StartAsync() => Task.CompletedTask;
    public Task StopAsync() => Task.CompletedTask;
    public Task JoinGameRoomAsync(string gameId) => Task.CompletedTask;
    public Task LeaveGameRoomAsync(string gameId) => Task.CompletedTask;

    // For server-side, these methods do nothing as they'll use the direct service calls
    public Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request) => Task.FromResult<GameStateDTO?>(null);
    public Task<bool> JoinGameAsync(string gameId, JoinGameRequestDTO request) => Task.FromResult(false);
    public Task<bool> StartGameAsync(string gameId) => Task.FromResult(false);
    public Task<bool> SubmitGuessAsync(string gameId, string playerId, string guess) => Task.FromResult(false);
    public Task<bool> CallVoteAsync(string gameId) => Task.FromResult(false);
    public Task<bool> VotePlayerAsync(string gameId, string playerId, string targetPlayerId) => Task.FromResult(false);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
