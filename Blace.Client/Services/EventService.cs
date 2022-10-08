using Microsoft.JSInterop;

namespace Blace.Client.Services;

public class EventService
{
    public EventService()
    {
        Current = this;
    }

    private static EventService? Current { get; set; }

    public event Action<string>? KeyDown;

    [JSInvokable]
    public static void OnKeyDown(string key) => Current?.KeyDown?.Invoke(key);
}