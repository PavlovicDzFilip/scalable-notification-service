namespace Notifications.Consumer;

public class KillSwitch
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    private int _killswitchActivated = 0;

    public void Activate()
    {
        if (Interlocked.Exchange(ref _killswitchActivated, 1) == 0)
        {
            _cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1));
        }
    }
}