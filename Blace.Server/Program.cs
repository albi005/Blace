using Blace.Server;
using Blace.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Sentry.AspNetCore;
using Constants = Blace.Server.Constants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddCors();

string? connectionString = builder.Configuration["CosmosDb:ConnectionString"];
if (connectionString != null)
{
    try
    {
        CosmosDbPlaceRepository repository = new(connectionString);
        await repository.Initialize();
        builder.Services.AddSingleton<IPlaceRepository>(repository);
    }
    catch (CosmosDbInitException e)
    {
        Console.WriteLine("Failed to initialize CosmosDB, storing data in memory instead: " + e.Message);
        builder.Services.AddSingleton<IPlaceRepository, InMemoryPlaceRepository>();
    }
}
else 
    builder.Services.AddSingleton<IPlaceRepository, InMemoryPlaceRepository>();

builder.Services.AddSingleton<PlaceService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<ScoreboardService>();
builder.Services.AddSingleton<StateService>();
builder.Services.AddSingleton<QuestionRepository>();
builder.Services.AddSingleton<QuestionService>();
builder.Services.AddSingleton<VoteService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();
builder.Services
    .AddSignalR(o => o.MaximumReceiveMessageSize = null)
    .AddMessagePackProtocol();

if (builder.Configuration["Sentry:Dsn"] != null)
{
    builder.WebHost.UseSentry(
        (Action<SentryAspNetCoreOptions>)builder.Configuration.GetSection("Sentry").Bind);
}

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

await app.Services.GetRequiredService<PlaceService>().Initialize();

app.UseCors(cors => cors
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

if (!app.Environment.IsDevelopment())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapHub<Server>("/Game");

app.Run();