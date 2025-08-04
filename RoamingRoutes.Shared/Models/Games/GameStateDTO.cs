namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Describes the public state of a game room - this is what all players can see
/// </summary>
public class GameStateDTO
{
    /// <summary>
    /// The unique game ID for this game
    /// </summary>
    public string GameId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the host player
    /// </summary>
    public string HostPlayerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the game
    /// </summary>
    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;
    
    /// <summary>
    /// List of all players in the game (public information only)
    /// </summary>
    public List<PlayerDTO> Players { get; set; } = new();
    
    /// <summary>
    /// Current round number
    /// </summary>
    public int CurrentRound { get; set; } = 0;
    
    /// <summary>
    /// Maximum number of players allowed in this game
    /// </summary>
    public int MaxPlayers { get; set; } = 8;
    
    /// <summary>
    /// When this game was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this game was started (null if not started yet)
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// Whether the game is currently in voting phase
    /// </summary>
    public bool VotingPhase { get; set; } = false;
    
    /// <summary>
    /// When the current round started
    /// </summary>
    public DateTime RoundStartTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Recent guesses made in the game (for activity feed)
    /// </summary>
    public List<GuessDTO> RecentGuesses { get; set; } = new();
}
