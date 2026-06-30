using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MapInator.App;
using MapInator.App.Services;
using MapInator.Shared.Translations;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<MapInteropService>();
builder.Services.AddScoped<AppTranslationService>();
builder.Services.AddScoped<ITranslationService>(sp => sp.GetRequiredService<AppTranslationService>());

await builder.Build().RunAsync();
