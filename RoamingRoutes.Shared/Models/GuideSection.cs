namespace RoamingRoutes.Shared.Models;

public class GuideSection
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public List<Highlight> Highlights { get; set; } = new();
    public int CityGuideId { get; set; }
    public CityGuide? CityGuide { get; set; }
}
