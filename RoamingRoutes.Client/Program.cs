using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RoamingRoutes.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

// Register the game client service
builder.Services.AddScoped<IGameClientService, GameClientService>();

// Register the SignalR service
builder.Services.AddScoped<IGameSignalRService, GameSignalRService>();

// Register the word pair client service
builder.Services.AddScoped<IWordPairClientService, WordPairClientService>();

await builder.Build().RunAsync();
