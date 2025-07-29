using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoamingRoutes.Data;
using RoamingRoutes.Shared.Models;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;



  public TripsController(AppDbContext context, IWebHostEnvironment env) // Injecteer de service
    {
        _context = context;
        _env = env;
    }

    // GET: api/trips
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
    {
        var trips = await _context.Trips.ToListAsync();
        foreach (var trip in trips)
        {
            FindAndAssignOverviewPhotos(trip);
        }
        return trips;
    }

    // GET: api/trips/kyrgyzstan-adventure
   [HttpGet("{urlKey}")]
    public async Task<ActionResult<Trip>> GetTrip(string urlKey)
    {
        var trip = await _context.Trips
                                 .Include(t => t.Locations)
                                 .FirstOrDefaultAsync(t => t.UrlKey.ToLower() == urlKey.ToLower());

        if (trip == null) return NotFound();

        FindAndAssignOverviewPhotos(trip);
        FindAndAssignDailyPhotos(trip);

        return trip;
    }

    // GET: api/trips/countries
    [HttpGet("countries")]
    public async Task<ActionResult<IEnumerable<object>>> GetTripCountries()
    {
         var tripCountries = await _context.Trips
        .Where(t => !string.IsNullOrEmpty(t.CountryCode))
        .Select(t => new { countryCode = t.CountryCode, urlKey = t.UrlKey, title = t.Title })
        .Distinct()
        .ToListAsync();

        return Ok(tripCountries);
    }

    // GET: api/trips/kyrgyzstan-adventure/geojson
    [HttpGet("{UrlKey}/geojson")]
    public async Task<IActionResult> GetTripGeoJson(string UrlKey)
    {
        var trip = await _context
            .Trips.Include(t => t.Locations)
            .FirstOrDefaultAsync(t => t.UrlKey == UrlKey);
        if (trip == null || !trip.Locations.Any())
        {
            return NotFound();
        }

        var lineString = new
        {
            type = "Feature",
            geometry = new
            {
                type = "LineString",
                coordinates = trip
                    .Locations.OrderBy(l => l.Day)
                    .Select(l => new[] { l.Longitude, l.Latitude })
                    .ToArray(),
            },
        };

        var points = trip
            .Locations.Select(l => new
            {
                type = "Feature",
                geometry = new { type = "Point", coordinates = new[] { l.Longitude, l.Latitude } },
                properties = new
                {
                    day = l.Day,
                    description = l.Description,
                    activities = l.Activities,
                },
            })
            .ToList();

        var featureCollection = new
        {
            type = "FeatureCollection",
            features = new List<object> { lineString }.Concat(points),
        };

        return Ok(featureCollection);
    }

    // GET: api/trips/random
    [HttpGet("random")]
    public async Task<ActionResult<object>> GetRandomTripKey()
    {
        var allKeys = await _context.Trips.Select(t => t.UrlKey).ToListAsync();
        if (!allKeys.Any())
        {
            return NotFound();
        }

        var random = new Random();
        var randomIndex = random.Next(allKeys.Count);
        var randomKey = allKeys[randomIndex];

        return Ok(new { urlKey = randomKey });
    }

    private void FindAndAssignOverviewPhotos(Trip trip)
    {
        var tripImageDirectory = Path.Combine(
            _env.WebRootPath,
            "images",
            "trips",
            trip.UrlKey ?? ""
        );
        if (Directory.Exists(tripImageDirectory))
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var headerFile = Directory
                .GetFiles(tripImageDirectory, "header-*")
                .FirstOrDefault(file =>
                    allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant())
                );

            if (headerFile != null)
            {
                trip.HeaderImageUrl = $"/images/trips/{trip.UrlKey}/{Path.GetFileName(headerFile)}";
            }

            trip.OverviewPhotoUrls = Directory
                .GetFiles(tripImageDirectory, "overview-*")
                .Where(file =>
                    allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant())
                )
                .Select(path => $"/images/trips/{trip.UrlKey}/{Path.GetFileName(path)}")
                .ToList();
        }
    }

    private void FindAndAssignDailyPhotos(Trip trip)
    {
        var tripImageDirectory = Path.Combine(
            _env.WebRootPath,
            "images",
            "trips",
            trip.UrlKey ?? ""
        );
        if (Directory.Exists(tripImageDirectory))
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            foreach (var location in trip.Locations)
            {
                location.PhotoUrls = Directory
                    .GetFiles(tripImageDirectory, $"day{location.Day}-*")
                    .Where(file =>
                        allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant())
                    )
                    .Select(path => $"/images/trips/{trip.UrlKey}/{Path.GetFileName(path)}")
                    .ToList();
            }
        }
    }
}
