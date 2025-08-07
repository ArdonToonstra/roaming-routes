# Roaming Routes AI Coding Guide

## Architecture Overview

This is an ASP.NET Core 8 Blazor WebAssembly application with a clear server/client separation:
- **Server** (`RoamingRoutes/`): ASP.NET Core host providing JSON APIs, Entity Framework data layer, and content management
- **Client** (`RoamingRoutes.Client/`): Blazor WASM frontend with interactive components and real-time gaming features
- **Shared** (`RoamingRoutes.Shared/`): Common models and DTOs

Critical: The app uses `InteractiveWebAssemblyRenderMode(prerender: false)` globally to prevent HttpClient errors since all data fetching happens client-side.

## Content Management System

**YAML-Driven Content**: Content is managed through `.yaml` files, not a traditional CMS:
- Trip data: `RoamingRoutes/Data/Trips/*.yaml` 
- City guides: `RoamingRoutes/Data/Cities/*.yaml`
- Media: `wwwroot/images/{trips|cities}/{url_key}/` (naming convention matches YAML `url_key`)

**DataSeeder Process**: On startup, `DataSeeder.cs` parses YAML files using YamlDotNet and seeds SQLite database. In production, Google Drive sync downloads content first.

**Dynamic Properties**: Models use `[NotMapped]` attributes for calculated properties like `HeaderImageUrl` and `PhotoUrls` that are populated by controllers at runtime.

## Key Development Patterns

**Service Registration**: Dual registration pattern for client/server rendering:
```csharp
// Server-side services
builder.Services.AddScoped<IGameClientService, ServerSideGameClientService>();
// Client-side services (in Client/Program.cs)
builder.Services.AddScoped<IGameClientService, GameClientService>();
```

**JavaScript Interop**: Organized in `wwwroot/js/site.js` with namespaced objects:
- `window.roamingRoutesMap`: Leaflet map functionality
- `window.roamingRoutesWorldMap`: World map with trip navigation
- `window.roamingRoutesGeneral`: UI helpers (carousels, scrolling)

**SignalR Integration**: Real-time gaming features use SignalR with hub at `/gameHub` and dedicated service abstractions for client/server environments.

## Database & Models

**SQLite + EF Core**: Simple setup with migrations in `Migrations/`. Key entities:
- `Trip` → `Location` (1:many)  
- `CityGuide` → `GuideSection` → `Highlight` (nested 1:many)

**API Conventions**: Controllers follow REST patterns (`/api/trips`, `/api/trips/{urlKey}/geojson`) with JSON cycle handling configured.

## Frontend Architecture

**Component Structure**:
- Pages in `Pages/` use `@page` directives for routing
- Reusable components in `Components/` (Map.razor, DailyMapView.razor)
- Layout in `Components/Layout/MainLayout.razor`

**Styling**: Tailwind CSS primary, custom `app.css` for branding. Bootstrap included but minimal usage.

**State Management**: Component-level state with service injection. Game state managed through SignalR services.

## Development Workflow

**Running**: Use `dotnet run` from `RoamingRoutes/` directory (port 5216/7050)
**Database**: Migrations auto-apply on startup. Manual: `dotnet ef migrations add` / `dotnet ef database update`
**Content**: Add YAML files to `Data/` folders, restart app to seed database
**Debugging**: Client debugging via browser dev tools, server debugging in VS Code

## Gaming Features

The app includes a "Rounds" game system with:
- Local single-player games (`/game/local`)
- Online multiplayer via SignalR (`/game/lobby`, `/game/{gameId}`)
- Game state managed by `GameService` with concurrent dictionary storage
- Separate service abstractions for client/server rendering contexts
