namespace RoamingRoutes.Shared.Models;

public class Highlight
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Costs { get; set; }
    public string? References { get; set; }
    public int GuideSectionId { get; set; }
    public GuideSection? GuideSection { get; set; }
}
