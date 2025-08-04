using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Client.Services;

public interface IGameClientService
{
    Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request);
    Task<GameStateDTO?> JoinGameAsync(string gameId, JoinGameRequestDTO request);
    Task<GameStateDTO?> GetGameStateAsync(string gameId);
    Task<List<GameStateDTO>> GetAvailableGamesAsync();
    Task<bool> CheckHealthAsync();
}
