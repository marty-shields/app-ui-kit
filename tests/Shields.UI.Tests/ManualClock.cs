namespace Shields.UI.Tests;

/// <summary>A clock that only moves on <see cref="Advance"/>; timers fire, in due order, as it passes them.</summary>
internal sealed class ManualClock : TimeProvider
{
    private readonly List<ManualTimer> timers = [];
    private TimeSpan now;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => now.Ticks;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    public void Advance(TimeSpan by)
    {
        now += by;
        while (timers.Where(t => t.Due <= now).OrderBy(t => t.Due).FirstOrDefault() is { } due)
        {
            due.Fire();
        }
    }

    private sealed class ManualTimer(ManualClock clock, TimerCallback callback, object? state) : ITimer
    {
        public TimeSpan Due { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            clock.timers.Remove(this);
            if (dueTime != Timeout.InfiniteTimeSpan)
            {
                Due = clock.now + dueTime;
                clock.timers.Add(this);
            }

            return true;
        }

        public void Fire()
        {
            clock.timers.Remove(this);
            callback(state);
        }

        public void Dispose() => clock.timers.Remove(this);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
