window.roamingRoutesWorldMap = {
    map: null,
    geoJsonLayer: null,
    navigationHelper: null,
    
    initialize: async function (elementId, navigationHelper) {
        console.log("=== WorldMap Initialize Started ===");
        console.log("Element ID:", elementId);
        console.log("Navigation Helper:", navigationHelper);
        
        this.navigationHelper = navigationHelper;

        if (this.map) {
            console.log("Removing existing map");
            this.map.remove();
        }

        const container = document.getElementById(elementId);
        console.log("Container found:", container);
        if (!container) {
            console.error(`Map container #${elementId} not found.`);
            return;
        }

        console.log("Creating Leaflet map...");
        try {
            this.map = L.map(elementId, {
                center: [20, 0],
                zoom: 2,
                maxBounds: [[-90, -180], [90, 180]],
                maxBoundsViscosity: 1.0
            });
            console.log("Leaflet map created successfully");
        } catch (error) {
            console.error("Error creating Leaflet map:", error);
            return;
        }

        console.log("Adding tile layer...");
        try {
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                minZoom: 2,
                noWrap: true
            }).addTo(this.map);
            console.log("Tile layer added successfully");
        } catch (error) {
            console.error("Error adding tile layer:", error);
            return;
        }

        try {
            // Step 1: Fetch trip data
            const response = await fetch('/api/trips/countries');
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const trips = await response.json();

            // Step 2: Load and display countries
            await this.loadAndDisplayCountries(trips);
            
        } catch (error) {
            console.error("Error in WorldMap initialize:", error);
        }
    },

    loadAndDisplayCountries: async function(trips) {
        try {
            // Fetch the world countries GeoJSON
            const geoResponse = await fetch('/countries.json');
            if (!geoResponse.ok) {
                throw new Error(`Failed to load countries.json: ${geoResponse.status}`);
            }
            
            const countriesGeoJson = await geoResponse.json();
            
            // Create a map of trip country codes for quick lookup (using 3-letter codes)
            const tripCountries3Letter = new Set(trips.map(t => t.countryCode));
            
            // Create a map for trip data lookup
            const tripDataMap = new Map();
            trips.forEach(trip => {
                tripDataMap.set(trip.countryCode, trip);
            });
            
            // Add the GeoJSON layer with styling
            this.geoJsonLayer = L.geoJSON(countriesGeoJson, {
                style: (feature) => {
                    const countryCode3 = feature.id;
                    const hasTrip = tripCountries3Letter.has(countryCode3);
                    
                    return {
                        fillColor: hasTrip ? '#F97316' : '#E5E7EB', // Orange color for trips
                        weight: 1,
                        opacity: 1,
                        color: '#374151',
                        fillOpacity: hasTrip ? 0.7 : 0.1
                    };
                },
                onEachFeature: (feature, layer) => {
                    const countryCode3 = feature.id;
                    const tripData = tripDataMap.get(countryCode3);
                    
                    if (tripData) {
                        // Add hover effects
                        layer.on('mouseover', function(e) {
                            this.setStyle({
                                weight: 3,
                                fillOpacity: 0.9
                            });
                        });
                        
                        layer.on('mouseout', function(e) {
                            this.setStyle({
                                weight: 1,
                                fillOpacity: 0.7
                            });
                        });
                        
                        // Add click handler
                        layer.on('click', () => {
                            this.navigationHelper.invokeMethodAsync('NavigateToTrip', tripData.urlKey);
                        });
                        
                        // Add popup with trip title
                        layer.bindPopup(`
                            <div class="text-center">
                                <h3 class="font-bold">${tripData.title}</h3>
                                <p class="text-sm text-gray-600">Click to view our journey</p>
                            </div>
                        `);
                    }
                }
            }).addTo(this.map);
            
        } catch (error) {
            console.error("Error loading countries:", error);
            
            // Fallback: Create simple markers for countries
            this.createCountryMarkers(trips);
        }
    },

    createCountryMarkers: function(trips) {
        // Fallback method: create markers for countries
        // This is a simple fallback if GeoJSON loading fails
        const countryCoordinates = {
            'KG': [41.2044, 74.7661], // Kyrgyzstan
            'UZ': [41.3775, 64.5853], // Uzbekistan
            'KZ': [48.0196, 66.9237], // Kazakhstan
            'TJ': [38.8610, 71.2761], // Tajikistan
            // Add more country coordinates as needed
        };
        
        trips.forEach(trip => {
            const coords = countryCoordinates[trip.countryCode];
            if (coords) {
                L.marker(coords)
                    .addTo(this.map)
                    .bindPopup(`<b>Click to view our ${trip.countryCode} journey</b>`)
                    .on('click', () => {
                        this.navigationHelper.invokeMethodAsync('NavigateToTrip', trip.urlKey);
                    });
            }
        });
    },

    destroy: function () {
        if (this.geoJsonLayer) {
            this.geoJsonLayer.remove();
            this.geoJsonLayer = null;
        }
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
        this.navigationHelper = null;
    }
};


window.roamingRoutesMap = {
    maps: {}, // Object om kaart-instanties op te slaan
    routeLayers: {}, // Object om route-lagen op te slaan

    initializeAndDrawRoute: async function (elementId, geoJsonUrl) {
        // Extra controle om zeker te weten dat de container bestaat
        const container = document.getElementById(elementId);
        if (!container) {
            console.error(`Map container #${elementId} not found in DOM.`);
            return;
        }

        // Stap 1: Initialiseer de kaart als deze nog niet bestaat
        let map = this.maps[elementId];
        if (!map) {
            map = L.map(container).setView([40, 0], 2); // Gebruik de container variabele
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            }).addTo(map);
            this.maps[elementId] = map;
        }

        // Stap 2: Verwijder de oude route als die bestaat
        if (this.routeLayers[elementId]) {
            map.removeLayer(this.routeLayers[elementId]);
        }

        // Stap 3: Haal de nieuwe route-data op en teken deze
        try {
            const response = await fetch(geoJsonUrl);
            if (!response.ok) {
                console.error("Failed to fetch GeoJSON data:", response.status);
                return;
            }
            const data = await response.json();

            if (!data || !data.features || data.features.length === 0) {
                console.warn("GeoJSON data is empty or invalid.");
                return;
            }

            const routeLayer = L.geoJSON(data, {
                style: function (feature) {
                    if (feature.geometry.type === 'LineString') {
                        return { color: "#3388ff", weight: 5, opacity: 0.7 };
                    }
                },
                pointToLayer: function (feature, latlng) {
                    return L.circleMarker(latlng, {
                        radius: 6,
                        fillColor: "#ff7800",
                        color: "#000",
                        weight: 1,
                        opacity: 1,
                        fillOpacity: 0.8
                    });
                },
                onEachFeature: function (feature, layer) {
                    if (feature.properties && feature.properties.description) {
                        layer.bindPopup(`<b>Dag ${feature.properties.day}</b><br>${feature.properties.description}`);
                    }
                }
            }).addTo(map);

            this.routeLayers[elementId] = routeLayer;
            map.fitBounds(routeLayer.getBounds().pad(0.2)); // Zoom in op de getekende route
        } catch (error) {
            console.error("Error drawing route:", error);
        }
    },

    resetView: function (elementId) {
        const map = this.maps[elementId];
        const routeLayer = this.routeLayers[elementId];
        if (map && routeLayer) {
            map.fitBounds(routeLayer.getBounds().pad(0.1));
        }
    },

    highlightMarker: function (elementId, dayNumber) {
        const map = this.maps[elementId];
        const routeLayer = this.routeLayers[elementId];
        if (!map || !routeLayer) return;

        routeLayer.eachLayer(function (layer) {
            if (layer.feature && layer.feature.geometry.type === 'Point') {
                if (layer.feature.properties.day === dayNumber) {
                    map.setView(layer.getLatLng(), 13);
                    layer.openPopup();
                }
            }
        });
    },

        initDailyMap: function (elementId, lat, lon, day, zoomLevel) {
    // Verwijder eventuele oude kaart-instantie om conflicten te voorkomen
    if (this.maps[elementId]) {
        this.maps[elementId].remove();
    }

    const map = L.map(elementId, {
        scrollWheelZoom: false, // Start met scrollen uitgeschakeld
        dragging: false, // Start met slepen uitgeschakeld
    }).setView([lat, lon], zoomLevel || 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    L.marker([lat, lon]).addTo(map)
        .bindPopup(`<b>Dag ${day}</b>`)
        .openPopup();
    
    // Voeg een visuele hint toe dat de kaart niet interactief is
    const overlay = L.DomUtil.create('div', 'leaflet-interactive-overlay', map.getContainer());
    overlay.innerHTML = '<p>Klik om te activeren</p>';
    
    // Maak de kaart interactief na de eerste klik
    map.once('click', function () {
        map.scrollWheelZoom.enable();
        map.dragging.enable();
        L.DomUtil.remove(overlay); // Verwijder de hint
    });

    this.maps[elementId] = map;
},
        
    invalidateSize: function (elementId) {
        const map = this.maps[elementId];
        if (map) {
            // Vertelt Leaflet om de grootte opnieuw te berekenen
            map.invalidateSize();
        }
    },

};

window.roamingRoutesGeneral = {
    scrollToElement: (id) => {
        const element = document.getElementById(id);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    },

    scrollCarousel: (carouselId, direction) => {
        const carousel = document.getElementById(carouselId);
        if (carousel) {
            const scrollAmount = carousel.clientWidth / 2; // Scroll met de helft van de breedte
            carousel.scrollBy({ left: scrollAmount * direction, behavior: 'smooth' });
        }
    },

    initializeCarousel: (carouselId, dotNetHelper) => {
        const carousel = document.getElementById(carouselId);
        if (!carousel) return;

        const updateState = () => {
            if (!dotNetHelper) return;
            const tolerance = 5; // Aantal pixels tolerantie
            const canScrollLeft = carousel.scrollLeft > tolerance;
            const canScrollRight = carousel.scrollLeft < (carousel.scrollWidth - carousel.clientWidth - tolerance);
            dotNetHelper.invokeMethodAsync('UpdateCarouselState', carouselId, canScrollLeft, canScrollRight);
        };

        // Voeg een event listener toe die de staat update bij het scrollen
        carousel.addEventListener('scroll', updateState);

        // Stel de initiële staat in
        updateState();
    }
};

