using Timer = System.Timers.Timer;

namespace Blace.Server;

public class Throttler
{
    private readonly Action _action;
    private readonly Timer _timer;
    private bool _needsUpdate;
    
    public Throttler(double interval, Action action)
    {
        _action = action;
        _timer = new(interval) { AutoReset = false };
        _timer.Elapsed += (_, _) =>
        {
            if (_needsUpdate)
                UpdateCore();
        };
    }

    public void Update()
    {
        if (_timer.Enabled)
        {
            _timer.Enabled = true;
            _needsUpdate = true;
            return;
        }
        UpdateCore();
    }

    private void UpdateCore()
    {
        _needsUpdate = false;
        _timer.Start();
        _action();
    }
}