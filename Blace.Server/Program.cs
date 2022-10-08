using Blace.Server;
using Blace.Server.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Sentry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<CosmosDbOptions>()
    .Configure(builder.Configuration.GetSection("CosmosDb").Bind)
    .ValidateDataAnnotations();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCors();
builder.Services.AddSingleton<PlaceRepository>();
builder.Services.AddSingleton<PlaceService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<ScoreboardService>();
builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<QuestionRepository>();
builder.Services.AddSingleton<QuestionService>();
builder.Services.AddSingleton<VoteService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.WebHost.UseSentry(o =>
{
    o.Dsn = "https://2bc57b7722be4f619404cf7fbcec7213@o1311064.ingest.sentry.io/6766716";
    o.TracesSampleRate = 1.0;
});
builder.Services
    .AddSignalR(o => o.MaximumReceiveMessageSize = null)
    .AddMessagePackProtocol();
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);
    
WebApplication app = builder.Build();

await app.Services.GetRequiredService<PlaceRepository>().Initialize();
await app.Services.GetRequiredService<PlaceService>().Initialize();

if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (!context.Request.Path.StartsWithSegments("/Game"))
        {
            if (!context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-NAME"))
                return;
        }
        await next.Invoke();
    });

    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors(corsPolicyBuilder =>
{
    corsPolicyBuilder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host")
    .RequireAuthorization();

app.MapHub<Server>("/Game", o =>
{
    o.Transports = HttpTransportType.WebSockets;
    
});

app.Run();