using System.ComponentModel.DataAnnotations.Schema;

namespace RoamingRoutes.Shared.Models;

public class Location
{
    public int Id { get; set; }
    public int Day { get; set; }
    public string? Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public List<string> Activities { get; set; } = new(); // Lijst van activiteiten
    public string? Accommodation { get; set; } = "N/A"; // Accommodatie, standaard op "N/A" als er geen is
    public int TripId { get; set; }
    public Trip? Trip { get; set; }

    [NotMapped] // Vertel EF Core om deze eigenschap te negeren
    public List<string> PhotoUrls { get; set; } = new();
}
