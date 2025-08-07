using Microsoft.EntityFrameworkCore;
using RoamingRoutes.Client;
using RoamingRoutes.Components;
using RoamingRoutes.Data;
using RoamingRoutes.Services;
using RoamingRoutes.Client.Services;
using RoamingRoutes.Hubs;

var builder = WebApplication.CreateBuilder(args);
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=roamingroutes.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveWebAssemblyComponents();
builder.Services.AddSingleton<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IWordPairService, WordPairService>();

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Register game client service for server-side rendering
builder.Services.AddScoped<IGameClientService, ServerSideGameClientService>();

// Register SignalR service for server-side rendering
builder.Services.AddScoped<IGameSignalRService, ServerSideGameSignalRService>();

// Register word pair client service for server-side rendering
builder.Services.AddScoped<IWordPairClientService, ServerSideWordPairClientService>();

// Add HttpClient for server-side components
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    // For server-side rendering, use the local base address
    var context = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    if (context != null)
    {
        var request = context.Request;
        httpClient.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        // Fallback for when HttpContext is not available
        httpClient.BaseAddress = new Uri("http://localhost:5216");
    }
    return httpClient;
});

// Add HttpContextAccessor to access the current request context
builder.Services.AddHttpContextAccessor();

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

// Gebruik een enkele logger voor de hele applicatie-opstart
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Stap 1: Synchroniseer content alleen in productie
if (!app.Environment.IsDevelopment())
{
    logger.LogInformation("Production mode detected. Starting Google Drive synchronization...");
    using (var scope = app.Services.CreateScope())
    {
        var driveService = scope.ServiceProvider.GetRequiredService<IGoogleDriveService>();
        await driveService.SynchronizeContentAsync();
    }
}
else
{
    logger.LogInformation(
        "Development mode detected. Skipping Google Drive synchronization and using cached content."
    );
}

// Stap 2: Seed de database met de (zojuist gedownloade of gecachte) content
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var env = services.GetRequiredService<IWebHostEnvironment>();

        await dbContext.Database.MigrateAsync();
        await DataSeeder.SeedAsync(dbContext, logger, env.ContentRootPath);
    }
    catch (Exception ex)
    {
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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();
app.UseAntiforgery();

// Map SignalR hub
app.MapHub<GameHub>("/gameHub");

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RoamingRoutes.Client._Imports).Assembly);

app.Run();
