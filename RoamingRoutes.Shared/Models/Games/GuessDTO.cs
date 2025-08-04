namespace RoamingRoutes.Shared.Models.Games;

public class GuessDTO
{
    public string PlayerId { get; set; } = string.Empty;
    public string TargetPlayerId { get; set; } = string.Empty;
    public string GuessedWord { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime Timestamp { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string TargetPlayerName { get; set; } = string.Empty;
}
