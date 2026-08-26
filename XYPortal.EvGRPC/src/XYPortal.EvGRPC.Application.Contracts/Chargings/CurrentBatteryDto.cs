using System;

namespace XYPortal.EvGRPC.Chargings;

/// <summary>
/// Snapshot of the current battery state for a single vehicle.
/// Returned by <c>IChargingAppService.GetCurrentBatteryAsync</c> so
/// the UI can render the "battery %" tile without iterating the
/// full charging history.
/// </summary>
public class CurrentBatteryDto
{
    /// <summary>0..100; 0 means "no charging recorded yet".</summary>
    public int BatteryPercent { get; set; }
    public DateTimeOffset? LastChargingEndTime { get; set; }
}
