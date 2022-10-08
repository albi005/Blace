using Blazored.LocalStorage;
using Blace.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace Blace.Client.Services;

public class HubService
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IServiceProvider _serviceProvider;
    
    public HubService(IWebAssemblyHostEnvironment env, ILocalStorageService localStorageService, IServiceProvider serviceProvider)
    {
        _localStorageService = localStorageService;
        _serviceProvider = serviceProvider;
        Connection = new HubConnectionBuilder()
            .WithUrl(
                env.BaseAddress.Contains("localhost")
                    ? "http://localhost:7151/Game"
                    : env.BaseAddress.Contains("192.168.0.5")
                        ? "http://192.168.0.5:7151/Game"
                        : "https://Blaceserver20220918222136.azurewebsites.net/Game",
                o => o.AccessTokenProvider = () => Task.FromResult(UserId.ToString())!)
            .AddMessagePackProtocol()
            .WithAutomaticReconnect()
            .Build();
        Server = Connection.GetServerProxy<IServer>();
    }

    public HubConnection Connection { get; }
    public IServer Server { get; }
    public Guid UserId { get; private set; }
    
    public async Task Start()
    {
        foreach (IClient client in _serviceProvider.GetRequiredService<IEnumerable<IClient>>())
            RegisterClient(client);

        if (await _localStorageService.ContainKeyAsync("userid"))
            UserId = await _localStorageService.GetItemAsync<Guid>("userid");
        else
            await _localStorageService.SetItemAsync(
                "userid",
                UserId = Guid.NewGuid());
        
        await Connection.StartAsync();
    }

#pragma warning disable IDE0001
    // ReSharper disable once RedundantTypeArgumentsOfMethod
    public IDisposable RegisterClient(IClient client) => Connection.RegisterClient<IClient>(client);
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubServerProxyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
internal class HubClientProxyAttribute : Attribute
{
}

internal static partial class MyCustomExtensions
{
    [HubClientProxy]
    public static partial IDisposable RegisterClient<T>(this HubConnection connection, T provider);

    [HubServerProxy]
    public static partial T GetServerProxy<T>(this HubConnection connection);
}
