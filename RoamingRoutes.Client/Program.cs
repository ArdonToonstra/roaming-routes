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

await builder.Build().RunAsync();
