namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Request DTO for joining an existing game
/// </summary>
public class JoinGameRequestDTO
{
    /// <summary>
    /// The nickname the player wants to use
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for joining a game
/// </summary>
public class JoinGameResponseDTO
{
    /// <summary>
    /// The player ID assigned to the joining player
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the join was successful
    /// </summary>
    public bool Success { get; set; } = false;
    
    /// <summary>
    /// Error message if join failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// The current game state
    /// </summary>
    public GameStateDTO? GameState { get; set; }
}
