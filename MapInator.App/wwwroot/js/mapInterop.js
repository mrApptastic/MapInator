window.mapInterop = (function () {
    var map = null;
    // filePath → L.geoJSON layer
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
        map = L.map(elementId, {
            center: [56.0, 10.0],
            zoom: 7
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }).addTo(map);
    }

    function addGeoJsonLayer(filePath) {
        if (!map) return;
        if (layers[filePath]) return; // already loaded
        fetch(filePath)
            .then(function (r) { return r.json(); })
            .then(function (geojson) {
                var layer = L.geoJSON(geojson, {
                    pointToLayer: function (feature, latlng) {
                        var hoofdtype = (feature.properties && feature.properties.hoofdtype) || '';
                        var colour = colourFor(hoofdtype);
                        return L.circleMarker(latlng, {
                            radius: 5,
                            fillColor: colour,
                            fillOpacity: 0.85,
                            color: '#fff',
                            weight: 1
                        });
                    },
                    onEachFeature: function (feature, layer) {
                        if (!feature.properties) return;
                        var props = feature.properties;
                        var navn = props.navn || '';
                        var type = props.undertype || props.hoofdtype || '';
                        var kommuner = props.kommuner || [];
                        var content =
                            '<div style="font-family:sans-serif;max-width:220px">' +
                            '<strong style="font-size:14px">' + escHtml(navn) + '</strong>' +
                            '<div class="text-muted" style="font-size:12px">' + escHtml(type) + '</div>' +
                            (kommuner.length ? '<div style="font-size:11px;margin-top:4px">' + kommuner.map(escHtml).join(', ') + '</div>' : '') +
                            '</div>';
                        layer.bindPopup(content);
                    },
                    style: function (feature) {
                        var hoofdtype = (feature.properties && feature.properties.hoofdtype) || '';
                        var colour = colourFor(hoofdtype);
                        return {
                            color: colour,
                            weight: 2,
                            opacity: 0.85,
                            fillColor: colour,
                            fillOpacity: 0.5
                        };
                    }
                });
                layer.addTo(map);
                layers[filePath] = layer;
            })
            .catch(function (err) {
                console.warn('MapInator: could not load layer', filePath, err);
            });
    }

    function removeGeoJsonLayer(filePath) {
        if (!map || !layers[filePath]) return;
        map.removeLayer(layers[filePath]);
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
