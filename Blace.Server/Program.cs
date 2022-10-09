using Blace.Server;
using Blace.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Constants = Blace.Server.Constants;

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

if (builder.Environment.IsDevelopment())
    builder.Services.AddAuthorization(o => o.AddPolicy(Constants.AdminPolicy, p => p.RequireAssertion(_ => true)));
else
{
    Environment.SetEnvironmentVariable("WEBSITE_AUTH_DEFAULT_PROVIDER", "AAD");
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
        .EnableTokenAcquisitionToCallDownstreamApi();
    builder.Services.AddAuthorization(o => o.AddPolicy(Constants.AdminPolicy, p => p.RequireClaim("roles", "Admin")));
}

WebApplication app = builder.Build();

await app.Services.GetRequiredService<PlaceRepository>().Initialize();
await app.Services.GetRequiredService<PlaceService>().Initialize();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors(cors => cors
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

if (!app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHub<Server>("/Game");

app.Run();