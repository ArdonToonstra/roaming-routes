namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Response DTO for getting a player's private information
/// </summary>
public class PlayerInfoResponseDTO
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; } = false;
    
    /// <summary>
    /// Error message if request failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's private information
    /// </summary>
    public MyPlayerInfoDTO? PlayerInfo { get; set; }
    
    /// <summary>
    /// The current public game state
    /// </summary>
    public GameStateDTO? GameState { get; set; }
}
