window.roamingRoutesWorldMap = {
    map: null,
    geoJsonLayer: null,
    navigationHelper: null, // To call Blazor methods

    initialize: async function (elementId, navigationHelper) {
        console.log("Attempting to initialize world map...");
        this.navigationHelper = navigationHelper;

        if (this.map) {
            this.map.remove();
        }

        const container = document.getElementById(elementId);
        if (!container) {
            console.error(`Map container #${elementId} not found.`);
            return;
        }

        this.map = L.map(elementId, {
            center: [20, 0],
            zoom: 2,
            maxBounds: [[-90, -180], [90, 180]],
            maxBoundsViscosity: 1.0
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            minZoom: 2,
            noWrap: true
        }).addTo(this.map);

        try {
            // Step 1: Fetch trip data
            console.log("Fetching trip country data from /api/trips/countries");
            const tripsResponse = await fetch('/api/trips/countries');
            if (!tripsResponse.ok) {
                console.error("Failed to fetch trip countries. Status:", tripsResponse.status);
                return;
            }
            const trips = await tripsResponse.json();
            const tripCountries = trips.map(t => t.countryCode);
            console.log("Found trip countries:", tripCountries);

            // Step 2: Fetch the world map GeoJSON data
            console.log("Fetching GeoJSON data from countries.geojson");
            const geoJsonResponse = await fetch('countries.geojson');
             if (!geoJsonResponse.ok) {
                console.error("Failed to fetch countries.geojson. Status:", geoJsonResponse.status);
                alert("Error: Could not load the world map data (countries.geojson). Please ensure the file exists in the wwwroot folder.");
                return;
            }
            const geoJsonData = await geoJsonResponse.json();
            console.log("Successfully loaded countries.geojson");


            this.geoJsonLayer = L.geoJSON(geoJsonData, {
                style: (feature) => {
                    // *** THIS IS THE KEY FIX ***
                    const countryCode = feature.properties.iso_a2; // Correctly access the ISO code
                    const hasTrip = tripCountries.includes(countryCode);
                    return {
                        fillColor: hasTrip ? '#FD8D3C' : '#BDBDBD',
                        weight: 1,
                        opacity: 1,
                        color: 'white',
                        fillOpacity: 0.8
                    };
                },
                onEachFeature: (feature, layer) => {
                    const countryCode = feature.properties.iso_a2; // Use the same correct property here
                    const trip = trips.find(t => t.countryCode === countryCode);
                    if (trip) {
                        layer.on({
                            mouseover: (e) => {
                                e.target.setStyle({ weight: 2, color: '#E65100', fillOpacity: 0.9 });
                            },
                            mouseout: (e) => {
                                this.geoJsonLayer.resetStyle(e.target);
                            },
                            click: () => {
                                this.navigationHelper.invokeMethodAsync('NavigateToTrip', trip.urlKey);
                            }
                        });
                        layer.bindTooltip(feature.properties.name, { sticky: true });
                    }
                }
            }).addTo(this.map);
            console.log("World map drawn successfully.");
        } catch (error) {
            console.error("A critical error occurred during world map initialization:", error);
        }
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

