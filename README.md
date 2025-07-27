# Technical Summary: Roaming Routes Project

This document provides a detailed technical overview of the architecture, technology stack, and core functionalities of the Roaming Routes website.

## 1. Architecture and Technology Stack

The project is set up as a modern, API-first web application with a clear separation between the backend and frontend.

- **Hosting Model:** An ASP.NET Core Web App serves as the host and API backend.
- **Frontend Framework:** A Blazor WebAssembly (WASM) application is hosted by the ASP.NET Core app and runs entirely in the client's browser.
- **Rendering Mode:** The application is globally configured with an `InteractiveWebAssemblyRenderMode(prerender: false)` in `App.razor`. This is a crucial setting that disables server-side pre-rendering to prevent HttpClient-related errors, since all data fetching occurs on the client.

### Core Technologies:

- **Backend:** C# with .NET 8
- **Database:** SQLite, managed via Entity Framework Core (EF Core)
- **Styling:** Tailwind CSS for layout, supplemented with a custom `site.css` for branding (fonts, color scheme).
- **Map Visualization (Interactive):** Leaflet.js, with map tiles from OpenStreetMap.
- **Data Parsing:** YamlDotNet for processing .yaml content files.

## 2. Content Workflow: YAML-driven

The website's content is not managed through a traditional CMS, but via a file-based workflow.

**Data Source:** The source of all content are .yaml files placed in the `RoamingRoutes/Data/` folder of the server project (in subfolders `Trips` and `Cities`).

**Data Seeding:** A `DataSeeder.cs` service on the server is executed during application startup. This service:
- Scans the `Data/Trips` and `Data/Cities` folders for `.yaml` or `.yml` files.
- Uses YamlDotNet to parse the files into temporary C# helper classes.
- Checks per item (based on the unique `url_key`) whether it already exists in the SQLite database.
- Adds new items to the database.

**Media Management:** Photos are managed via a naming convention. They are placed in `RoamingRoutes/wwwroot/images/` in subfolders that exactly match the `url_key` of the corresponding trip or city guide (e.g., `images/trips/kyrgyzstan-adventure/`). The API finds and links these photos dynamically.

## 3. Backend Functionality

The backend is primarily a JSON API that provides data to the Blazor WASM frontend.

### Data Models (RoamingRoutes.Shared/Models):
- **Trip & Location:** Models for travel routes, with a one-to-many relationship.
- **CityGuide, GuideSection & Highlight:** Models for city guides, with nested one-to-many relationships.
- **[NotMapped] Attributes:** Dynamically calculated properties, such as `HeaderImageUrl` and `PhotoUrls`, are explicitly excluded from database mapping to prevent errors.

### API Controllers (RoamingRoutes/Controllers):
- **TripsController.cs:** Provides endpoints for retrieving all trips (`GET /api/trips`) and a specific trip based on the urlKey (`GET /api/trips/{urlKey}`). The GET endpoint for a specific trip dynamically enriches the object with found photo URLs. Also contains an endpoint for GeoJSON data (`GET /api/trips/{urlKey}/geojson`).
- **CityGuidesController.cs:** Similar structure for retrieving city guides.

**Database Context (AppDbContext.cs):** Defines the DbSets for Trips and CityGuides and is used by the controllers and DataSeeder.

## 4. Frontend Functionality

The frontend is built from reusable Blazor components and makes extensive use of JavaScript Interop for map functionality.

### Main Components (RoamingRoutes.Client/Pages & Components):
- **Trips.razor:** Serves as the homepage (`@page "/"`) and displays an overview of all trips.
- **TripDetail.razor:** A complex detail page with three views (Overview, Timeline, Interactive) managed via a `currentView` variable.
- **CityGuides.razor & CityGuideDetail.razor:** Similar structure for city guides.
- **Map.razor:** The main component for the interactive Leaflet map. Displays a complete route based on a GeoJSON endpoint. Has an `InvalidateMapSize` method to render correctly in a modal.
- **DailyMapView.razor:** A smaller, reusable component that displays a zoomed-in, interactive map for one specific Location.

### JavaScript Interop (wwwroot/js/site.js):
- **roamingRoutesMap:** Contains all functions for initializing and manipulating Leaflet maps (both the main map and daily maps).
- **roamingRoutesGeneral:** Contains helper functions for UI interactions, such as smooth scrolling to sections (`scrollToElement`) and controlling photo carousels (`scrollCarousel`, `initializeCarousel`).

### Advanced UI Elements:
- **Expandable Map:** The main map on the TripDetail page is shown in a smaller container and can be expanded to a full-screen "modal".
- **Photo Carousel:** Photos in the "Timeline" view are displayed in a horizontal carousel with "smart" arrow buttons that only appear when scrolling is possible. The carousel state is managed in C# and updated via a JSInvokable method.