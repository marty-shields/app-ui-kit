namespace Shields.UI.Tests;

/// <summary>A clock that always says it is <paramref name="now"/>, in UTC.</summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
