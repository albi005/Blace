using Blazored.LocalStorage;
using Blace.Client;
using Blace.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sentry;

using IDisposable sdk = SentrySdk.Init(o =>
{
    o.Dsn = "https://e078f0e3db504ed9ad6d166767887f51@o1311064.ingest.sentry.io/6748344";
    o.TracesSampleRate = 1.0;
});

try
{
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddSingleton<HubService>();
    builder.Services.AddSingleton(s => s.GetRequiredService<HubService>().Server);
    builder.Services.AddSingleton<EventService>();
    builder.Services.AddHubClient<CooldownService>();
    builder.Services.AddHubClient<PlaceService>();
    builder.Services.AddHubClient<PlayerService>();
    builder.Services.AddBlazoredLocalStorageAsSingleton();

    builder.Logging.AddSentry(o => o.InitializeSdk = false);

    WebAssemblyHost app = builder.Build();

    await app.Services.GetRequiredService<HubService>().Start();

    await Task.WhenAll(
        app.Services.GetRequiredService<PlayerService>().Initialize(),
        app.Services.GetRequiredService<PlaceService>().Initialize(),
        app.Services.GetRequiredService<CooldownService>().Initialize()
    );

    await app.RunAsync();
}
catch (Exception e)
{
    SentrySdk.CaptureException(e);
    await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
    throw;
}