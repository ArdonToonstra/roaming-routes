namespace RoamingRoutes.Shared.Models.Games;

/// <summary>
/// Constants used throughout the game system
/// </summary>
public static class GameConstants
{
    /// <summary>
    /// Game status values
    /// </summary>
    public static class GameStatus
    {
        public const string Lobby = "Lobby";
        public const string InProgress = "InProgress";
        public const string Finished = "Finished";
    }
    
    /// <summary>
    /// Game phase values
    /// </summary>
    public static class GamePhase
    {
        public const string Waiting = "Waiting";
        public const string Discussion = "Discussion";
        public const string Voting = "Voting";
        public const string Results = "Results";
        public const string RoleReveal = "RoleReveal";
    }
    
    /// <summary>
    /// Player roles in the Undercover game
    /// </summary>
    public static class PlayerRoles
    {
        public const string Civilian = "Civilian";
        public const string Undercover = "Undercover";
        public const string MrWhite = "MrWhite";
    }
    
    /// <summary>
    /// Winning teams
    /// </summary>
    public static class WinningTeams
    {
        public const string Civilians = "Civilians";
        public const string Undercover = "Undercover";
        public const string MrWhite = "MrWhite";
    }
    
    /// <summary>
    /// Action types for game actions
    /// </summary>
    public static class ActionTypes
    {
        public const string SubmitDescription = "SubmitDescription";
        public const string Vote = "Vote";
        public const string StartNextRound = "StartNextRound";
        public const string EndDiscussion = "EndDiscussion";
    }
    
    /// <summary>
    /// Game configuration
    /// </summary>
    public static class GameConfig
    {
        public const int MinPlayers = 3;
        public const int MaxPlayers = 10;
        public const int DiscussionTimeSeconds = 300; // 5 minutes
        public const int VotingTimeSeconds = 60; // 1 minute
        public const int ResultsTimeSeconds = 15; // 15 seconds
        public const int RoomCodeLength = 6;
    }
}
