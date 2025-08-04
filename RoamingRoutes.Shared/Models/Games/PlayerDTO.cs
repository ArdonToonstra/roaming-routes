namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Describes a player publicly - this is what other players can see
/// </summary>
public class PlayerDTO
{
    /// <summary>
    /// A unique session/connection ID for the player
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The player's chosen nickname
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the player is still alive in the game
    /// </summary>
    public bool IsAlive { get; set; } = true;
    
    /// <summary>
    /// Whether this player is the host of the game room
    /// </summary>
    public bool IsHost { get; set; } = false;
}
