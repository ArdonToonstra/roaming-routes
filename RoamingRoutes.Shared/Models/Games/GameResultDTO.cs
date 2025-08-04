namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Contains the result information for a completed game
/// </summary>
public class GameResultDTO
{
    /// <summary>
    /// The winning team/role
    /// </summary>
    public string WinningTeam { get; set; } = string.Empty; // "Civilians", "Undercover", "MrWhite"
    
    /// <summary>
    /// List of player IDs who won the game
    /// </summary>
    public List<string> WinnerIds { get; set; } = new();
    
    /// <summary>
    /// A message explaining why this team won
    /// </summary>
    public string WinReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Final role reveals for all players
    /// </summary>
    public Dictionary<string, string> PlayerRoles { get; set; } = new();
    
    /// <summary>
    /// The words that were in play this game
    /// </summary>
    public Dictionary<string, string> RoleWords { get; set; } = new();
}
