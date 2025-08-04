namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Request DTO for performing game actions
/// </summary>
public class GameActionRequestDTO
{
    /// <summary>
    /// The ID of the player performing the action
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of action being performed
    /// </summary>
    public string ActionType { get; set; } = string.Empty; // "SubmitDescription", "Vote", "StartNextRound"
    
    /// <summary>
    /// The data associated with the action
    /// </summary>
    public Dictionary<string, object> ActionData { get; set; } = new();
}

/// <summary>
/// Response DTO for game actions
/// </summary>
public class GameActionResponseDTO
{
    /// <summary>
    /// Whether the action was successful
    /// </summary>
    public bool Success { get; set; } = false;
    
    /// <summary>
    /// Error message if action failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// The updated game state (if action was successful)
    /// </summary>
    public GameStateDTO? GameState { get; set; }
}
