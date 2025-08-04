using RoamingRoutes.Shared.Models.Games;
using System.Net.Http.Json;

namespace RoamingRoutes.Client.Services;

public class GameClientService : IGameClientService
{
    private readonly HttpClient _httpClient;

    public GameClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/game/create", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameStateDTO>();
            }
        }
        catch (Exception)
        {
            // Log error in a real application
        }
        return null;
    }

    public async Task<GameStateDTO?> JoinGameAsync(string gameId, JoinGameRequestDTO request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/game/{gameId}/join", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameStateDTO>();
            }
        }
        catch (Exception)
        {
            // Log error in a real application
        }
        return null;
    }

    public async Task<GameStateDTO?> GetGameStateAsync(string gameId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/game/{gameId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<GameStateDTO>();
            }
        }
        catch (Exception)
        {
            // Log error in a real application
        }
        return null;
    }

    public async Task<List<GameStateDTO>> GetAvailableGamesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/game/available");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<GameStateDTO>>() ?? new List<GameStateDTO>();
            }
        }
        catch (Exception)
        {
            // Log error in a real application
        }
        return new List<GameStateDTO>();
    }

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/game/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
