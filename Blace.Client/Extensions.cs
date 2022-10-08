using Blace.Shared;

namespace Blace.Client;

public static class Extensions
{
    public static void AddHubClient<T>(this IServiceCollection services) where T : class, IClient
    {
        services.AddSingleton<T>();
        services.AddTransient<IClient>(s => s.GetRequiredService<T>());
    }
}