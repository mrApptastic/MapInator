using Microsoft.JSInterop;

namespace MapInator.App.Services;

public class MapInteropService(IJSRuntime js)
{
    public ValueTask InitMapAsync(string elementId) =>
        js.InvokeVoidAsync("mapInterop.initMap", elementId);

    public ValueTask AddLayerAsync(string filePath) =>
        js.InvokeVoidAsync("mapInterop.addGeoJsonLayer", filePath);

    public ValueTask RemoveLayerAsync(string filePath) =>
        js.InvokeVoidAsync("mapInterop.removeGeoJsonLayer", filePath);
}
