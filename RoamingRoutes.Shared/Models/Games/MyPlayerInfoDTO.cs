namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Describes a player's private, secret information - only visible to the player themselves
/// </summary>
public class MyPlayerInfoDTO : PlayerDTO
{
    /// <summary>
    /// The player's secret role in the game
    /// </summary>
    public string Role { get; set; } = string.Empty; // "Civilian", "Undercover", "MrWhite"
    
    /// <summary>
    /// The secret word assigned to this player
    /// </summary>
    public string Word { get; set; } = string.Empty;
}
