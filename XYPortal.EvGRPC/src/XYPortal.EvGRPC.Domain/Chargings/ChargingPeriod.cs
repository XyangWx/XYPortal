using System;

namespace XYPortal.EvGRPC.Chargings;

/// <summary>
/// Value object: a half-open charging session
/// (<c>[Start, End)</c>). A valid period has <c>End > Start</c>; a
/// zero-length session is rejected because it has no measurable
/// duration and cannot report a meaningful <c>kWh/h</c> figure.
/// </summary>
public readonly record struct ChargingPeriod
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public ChargingPeriod(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new ArgumentException(
                $"ChargingPeriod end ({end:O}) must be strictly after start ({start:O}).",
                nameof(end));
        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;
}
