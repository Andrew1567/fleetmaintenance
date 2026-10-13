using System;

namespace FleetMaintenance.Infrastructure;

/// <summary>Годинник із фіксованою датою: для тестів і відтворюваних демонстрацій.</summary>
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    /// <summary>Створює годинник, що завжди показує північ заданої дати (UTC).</summary>
    /// <param name="date">Дата, яку показуватиме годинник.</param>
    public FixedTimeProvider(DateOnly date)
    {
        _now = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }

    /// <inheritdoc />
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow()
    {
        return _now;
    }
}
