namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Request DTO for creating a new game
/// </summary>
public class CreateGameRequestDTO
{
    /// <summary>
    /// The nickname of the player creating the game (they become the host)
    /// </summary>
    public string HostNickname { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of game to create (for future extensibility)
    /// </summary>
    public string GameType { get; set; } = "Undercover";
}

/// <summary>
/// Response DTO for creating a new game
/// </summary>
public class CreateGameResponseDTO
{
    /// <summary>
    /// The generated room code
    /// </summary>
    public string RoomCode { get; set; } = string.Empty;
    
    /// <summary>
    /// The player ID assigned to the host
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
    
    /// <summary>
    /// The initial game state
    /// </summary>
    public GameStateDTO GameState { get; set; } = new();
}
