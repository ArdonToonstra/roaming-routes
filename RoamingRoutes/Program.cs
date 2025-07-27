using Microsoft.EntityFrameworkCore;
using RoamingRoutes.Client; // Add this if App.razor is in RoamingRoutes.Client namespace
using RoamingRoutes.Client.Pages;
using RoamingRoutes.Components;
using RoamingRoutes.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=roamingroutes.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddSingleton<
    RoamingRoutes.Services.IGoogleDriveService,
    RoamingRoutes.Services.GoogleDriveService
>();

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System
            .Text
            .Json
            .Serialization
            .ReferenceHandler
            .IgnoreCycles;
    });

var app = builder.Build();

// Synchroniseer eerst de content van Google Drive
using (var scope = app.Services.CreateScope())
{
    var driveService =
        scope.ServiceProvider.GetRequiredService<RoamingRoutes.Services.IGoogleDriveService>();
    await driveService.SynchronizeContentAsync();
}

// Synchroniseer eerst de content van Google Drive
using (var scope = app.Services.CreateScope())
{
    var driveService =
        scope.ServiceProvider.GetRequiredService<RoamingRoutes.Services.IGoogleDriveService>();
    await driveService.SynchronizeContentAsync();
}

// Seed daarna de database met de zojuist gedownloade content
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        var env = services.GetRequiredService<IWebHostEnvironment>(); // Haal de environment op

        await dbContext.Database.MigrateAsync();

        // Geef het correcte pad mee aan de seeder
        await RoamingRoutes.Data.DataSeeder.SeedAsync(dbContext, logger, env.ContentRootPath);
    }
    catch (System.Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapControllers();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RoamingRoutes.Client.Pages.Trips).Assembly);

app.Run();
