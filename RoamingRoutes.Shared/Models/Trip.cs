using System.ComponentModel.DataAnnotations.Schema; // Voeg deze using toe

namespace RoamingRoutes.Shared.Models;

public class Trip
{
    public int Id { get; set; }
    public string UrlKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? CountryCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetTotal { get; set; }
    public string? BudgetCurrency { get; set; }
    public List<Location> Locations { get; set; } = new();

    [NotMapped] // Vertel EF Core om deze eigenschap te negeren
    public List<string> OverviewPhotoUrls { get; set; } = new();

    [NotMapped]
    public string? HeaderImageUrl { get; set; }
}
