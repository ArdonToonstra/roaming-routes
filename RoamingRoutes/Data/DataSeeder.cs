using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoamingRoutes.Shared.Models;
using YamlDotNet.Serialization;

namespace RoamingRoutes.Data;

public static class DataSeeder
{
    // --- Hulpklassen voor Trips ---
    private class YamlTripBudget
    {
        [YamlMember(Alias = "total")]
        public decimal Total { get; set; }

        [YamlMember(Alias = "currency")]
        public string Currency { get; set; } = "EUR";
    }

    private class YamlTripLocation
    {
        [YamlMember(Alias = "day")]
        public int Day { get; set; }

        [YamlMember(Alias = "date")]
        public string Date { get; set; } = "";

        [YamlMember(Alias = "location_name")]
        public string LocationName { get; set; } = "";

        [YamlMember(Alias = "location_gps_lat")]
        public double LocationGpsLat { get; set; }

        [YamlMember(Alias = "location_gps_lon")]
        public double LocationGpsLon { get; set; }

        [YamlMember(Alias = "activities")]
        public List<string> Activities { get; set; } = new();

        [YamlMember(Alias = "accommodation")]
        public object Accommodation { get; set; } = new(); // Gebruik object om zowel string als object te kunnen parsen
    }

    private class YamlTrip
    {
        [YamlMember(Alias = "url_key")]
        public string UrlKey { get; set; } = "";

        [YamlMember(Alias = "title")]
        public string Title { get; set; } = "";

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = "";

        [YamlMember(Alias = "start_date")]
        public System.DateTime StartDate { get; set; }

        [YamlMember(Alias = "end_date")]
        public System.DateTime EndDate { get; set; }

        [YamlMember(Alias = "country")]
        public string Country { get; set; } = "";

        [YamlMember(Alias = "budget")]
        public YamlTripBudget Budget { get; set; } = new();

        [YamlMember(Alias = "travel_itinerary")]
        public List<YamlTripLocation> TravelItinerary { get; set; } = new();
    }

    // --- Hulpklassen voor City Guides ---
    private class YamlHighlight
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = "";

        [YamlMember(Alias = "description")]
        public string Description { get; set; } = "";

        [YamlMember(Alias = "costs")]
        public string? Costs { get; set; }

        [YamlMember(Alias = "references")]
        public string? References { get; set; }
    }

    private class YamlGuideSection
    {
        [YamlMember(Alias = "title")]
        public string Title { get; set; } = "";

        [YamlMember(Alias = "icon")]
        public string Icon { get; set; } = "";

        [YamlMember(Alias = "highlights")]
        public List<YamlHighlight> Highlights { get; set; } = new();
    }

    private class YamlCityGuide
    {
        [YamlMember(Alias = "url_key")]
        public string UrlKey { get; set; } = "";

        [YamlMember(Alias = "city_name")]
        public string CityName { get; set; } = "";

        [YamlMember(Alias = "country")]
        public string Country { get; set; } = "";

        [YamlMember(Alias = "summary")]
        public string Summary { get; set; } = "";

        [YamlMember(Alias = "introduction")]
        public string Introduction { get; set; } = "";

        [YamlMember(Alias = "sections")]
        public List<YamlGuideSection> Sections { get; set; } = new();
    }

    public static async Task SeedAsync(AppDbContext context, ILogger logger, string contentRootPath)
    {
        await SeedTripsAsync(context, logger, contentRootPath);
        await SeedCityGuidesAsync(context, logger, contentRootPath);
    }

    private static async Task SeedCityGuidesAsync(
        AppDbContext context,
        ILogger logger,
        string contentRootPath
    )
    {
        var guidesDirectory = Path.Combine(contentRootPath, "_contentCache", "Cities");
        if (!Directory.Exists(guidesDirectory))
            return;
        var guideFiles = Directory
            .GetFiles(guidesDirectory, "*.yaml")
            .Concat(Directory.GetFiles(guidesDirectory, "*.yml"));
        if (!guideFiles.Any())
            return;

        logger.LogInformation("--- City Guide Seeder Started ---");
        var deserializer = new DeserializerBuilder().Build();
        var newGuidesFound = false;

        foreach (var filePath in guideFiles)
        {
            var yamlContent = await File.ReadAllTextAsync(filePath);
            var yamlGuide = deserializer.Deserialize<YamlCityGuide>(yamlContent);

            if (yamlGuide != null && !string.IsNullOrWhiteSpace(yamlGuide.UrlKey))
            {
                if (!await context.CityGuides.AnyAsync(g => g.UrlKey == yamlGuide.UrlKey))
                {
                    newGuidesFound = true;
                    var guideEntity = new CityGuide
                    {
                        UrlKey = yamlGuide.UrlKey,
                        CityName = yamlGuide.CityName,
                        Country = yamlGuide.Country,
                        Summary = yamlGuide.Summary,
                        Introduction = yamlGuide.Introduction,
                        Sections = yamlGuide
                            .Sections.Select(s => new GuideSection
                            {
                                Title = s.Title,
                                Icon = s.Icon,
                                Highlights = s
                                    .Highlights.Select(h => new Highlight
                                    {
                                        Name = h.Name,
                                        Description = h.Description,
                                        Costs = h.Costs,
                                        References = h.References,
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                    };
                    await context.CityGuides.AddAsync(guideEntity);
                }
            }
        }
        if (newGuidesFound)
            await context.SaveChangesAsync();
        logger.LogInformation("--- City Guide Seeder Finished ---");
    }

    private static async Task SeedTripsAsync(
        AppDbContext context,
        ILogger logger,
        string contentRootPath
    )
    {
        var tripsDirectory = Path.Combine(contentRootPath, "_contentCache", "Trips");
        if (!Directory.Exists(tripsDirectory))
            return;

        var tripFiles = Directory
            .GetFiles(tripsDirectory, "*.yaml")
            .Concat(Directory.GetFiles(tripsDirectory, "*.yml"));
        if (!tripFiles.Any())
            return;

        logger.LogInformation("--- Trip Seeder Started ---");

        // FIX: Configureer de deserializer om onbekende eigenschappen te negeren
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

        var newTripsFound = false;

        foreach (var filePath in tripFiles)
        {
            try
            {
                var yamlContent = await File.ReadAllTextAsync(filePath);
                var yamlTrip = deserializer.Deserialize<YamlTrip>(yamlContent);

                if (yamlTrip != null && !string.IsNullOrWhiteSpace(yamlTrip.UrlKey))
                {
                    if (!await context.Trips.AnyAsync(t => t.UrlKey == yamlTrip.UrlKey))
                    {
                        newTripsFound = true;
                        var tripEntity = new Trip
                        {
                            // ... (andere eigenschappen zoals UrlKey, Title, etc.) ...
                            Locations = yamlTrip
                                .TravelItinerary.Select(item =>
                                {
                                    string accommodationString = "";
                                    if (item.Accommodation is string simpleAccommodation)
                                    {
                                        accommodationString = simpleAccommodation;
                                    }
                                    else if (
                                        item.Accommodation
                                        is IDictionary<object, object> complexAccommodation
                                    )
                                    {
                                        // Converteer het complexe object naar een leesbare string
                                        accommodationString = string.Join(
                                            ", ",
                                            complexAccommodation.Select(kvp =>
                                                $"{kvp.Key}: {kvp.Value}"
                                            )
                                        );
                                    }

                                    return new Location
                                    {
                                        Day = item.Day,
                                        Date = item.Date,
                                        Description = item.LocationName,
                                        Latitude = item.LocationGpsLat,
                                        Longitude = item.LocationGpsLon,
                                        Activities = item.Activities,
                                        Accommodation = accommodationString, // FIX: Wijs de geconverteerde string toe
                                    };
                                })
                                .ToList(),
                        };
                        await context.Trips.AddAsync(tripEntity);
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error processing trip file: {FilePath}", filePath);
            }
        }
        if (newTripsFound)
            await context.SaveChangesAsync();
        logger.LogInformation("--- Trip Seeder Finished ---");
    }
}
