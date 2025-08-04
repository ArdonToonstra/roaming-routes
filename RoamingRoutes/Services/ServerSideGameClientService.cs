using RoamingRoutes.Shared.Models.Games;
using RoamingRoutes.Services;

namespace RoamingRoutes.Client.Services;

public class ServerSideGameClientService : IGameClientService
{
    private readonly IGameService _gameService;

    public ServerSideGameClientService(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request)
    {
        try
        {
            return await _gameService.CreateGameAsync(request);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GameStateDTO?> JoinGameAsync(string gameId, JoinGameRequestDTO request)
    {
        try
        {
            var joinResult = await _gameService.JoinGameAsync(gameId, request);
            if (joinResult != null)
            {
                // After successful join, get the updated game state
                return await _gameService.GetGameStateAsync(gameId);
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<GameStateDTO?> GetGameStateAsync(string gameId)
    {
        try
        {
            return await _gameService.GetGameStateAsync(gameId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<GameStateDTO>> GetAvailableGamesAsync()
    {
        try
        {
            return await _gameService.GetAvailableGamesAsync();
        }
        catch (Exception)
        {
            return new List<GameStateDTO>();
        }
    }

    public Task<bool> CheckHealthAsync()
    {
        // Since we're running server-side, we're always healthy if we get here
        return Task.FromResult(true);
    }
}
