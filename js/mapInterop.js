window.mapInterop = (function () {
    var map = null;
    // filePath → array of Feature objects added via map.data
    var layers = {};

    // Colour palette per hoofdtype (cycles if more types than colours)
    var palette = [
        '#e6194b', '#3cb44b', '#4363d8', '#f58231', '#911eb4',
        '#42d4f4', '#f032e6', '#bfef45', '#fabed4', '#469990',
        '#dcbeff', '#9a6324', '#fffac8', '#800000', '#aaffc3'
    ];
    var typeColours = {};
    var colourIndex = 0;

    function colourFor(hoofdtype) {
        if (!typeColours[hoofdtype]) {
            typeColours[hoofdtype] = palette[colourIndex % palette.length];
            colourIndex++;
        }
        return typeColours[hoofdtype];
    }

    function initMap(elementId) {
        if (map) return;
        map = new google.maps.Map(document.getElementById(elementId), {
            center: { lat: 56.0, lng: 10.0 },
            zoom: 7,
            mapTypeId: 'roadmap',
            streetViewControl: false,
            fullscreenControl: true
        });

        // Info window for clicked features
        var infoWindow = new google.maps.InfoWindow();

        map.data.addListener('click', function (event) {
            var props = event.feature;
            var navn = props.getProperty('navn') || '';
            var type = props.getProperty('undertype') || props.getProperty('hoofdtype') || '';
            var status = props.getProperty('navnestatus') || '';
            var kommuner = props.getProperty('kommuner') || [];
            var content =
                '<div style="font-family:sans-serif;max-width:220px">' +
                '<strong style="font-size:14px">' + escHtml(navn) + '</strong>' +
                '<div class="text-muted" style="font-size:12px">' + escHtml(type) + '</div>' +
                (kommuner.length ? '<div style="font-size:11px;margin-top:4px">' + kommuner.map(escHtml).join(', ') + '</div>' : '') +
                '</div>';
            infoWindow.setContent(content);
            infoWindow.setPosition(event.latLng);
            infoWindow.open(map);
        });

        map.data.setStyle(function (feature) {
            var hoofdtype = feature.getProperty('hoofdtype') || '';
            var colour = colourFor(hoofdtype);
            return {
                icon: {
                    path: google.maps.SymbolPath.CIRCLE,
                    scale: 5,
                    fillColor: colour,
                    fillOpacity: 0.85,
                    strokeColor: '#fff',
                    strokeWeight: 1
                }
            };
        });
    }

    function addGeoJsonLayer(filePath) {
        if (!map) return;
        if (layers[filePath]) return; // already loaded
        fetch(filePath)
            .then(function (r) { return r.json(); })
            .then(function (geojson) {
                var features = map.data.addGeoJson(geojson);
                layers[filePath] = features;
            })
            .catch(function (err) {
                console.warn('MapInator: could not load layer', filePath, err);
            });
    }

    function removeGeoJsonLayer(filePath) {
        if (!map || !layers[filePath]) return;
        layers[filePath].forEach(function (f) { map.data.remove(f); });
        delete layers[filePath];
    }

    function escHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    return { initMap: initMap, addGeoJsonLayer: addGeoJsonLayer, removeGeoJsonLayer: removeGeoJsonLayer };
})();
