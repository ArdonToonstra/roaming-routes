namespace RoamingRoutes.Shared.Models.Games
{
    public class WordPairCategory
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<WordPair> Pairs { get; set; } = new();
    }

    public class WordPair
    {
        public string Civilian { get; set; } = "";
        public string Undercover { get; set; } = "";
    }

    public class WordPairConfiguration
    {
        public List<WordPairCategory> Categories { get; set; } = new();
    }
}
