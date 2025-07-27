using System.ComponentModel.DataAnnotations.Schema;

namespace RoamingRoutes.Shared.Models;

public class CityGuide
{
    public int Id { get; set; }
    public string UrlKey { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public List<GuideSection> Sections { get; set; } = new();

    [NotMapped]
    public string? HeaderImageUrl { get; set; }
}
