using Microsoft.EntityFrameworkCore;
using RoamingRoutes.Shared.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Trip> Trips { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<CityGuide> CityGuides { get; set; }
}
