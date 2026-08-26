using System.Linq;
using System.Collections.Generic;
using System;

namespace XYPortal.EvGRPC.Vehicles;

/// <summary>
/// Aggregate root: a single electric vehicle tracked by the system.
/// Identity is the upstream evGRpc <c>id</c> (a server-issued string
/// — we deliberately do NOT remap to <c>Guid</c>; staying string
/// avoids an extra round-trip conversion and matches the wire
/// contract one-to-one).
///
/// This entity is 充血 (rich): every mutation goes through a method
/// that validates the invariant. Callers must never set properties
/// directly. EF persistence is wired via the private parameterless
/// constructor in Phase 2 (EFCore project); for now, the public
/// ctor is the only way to construct a <see cref="Vehicle"/>.
///
/// Invariants enforced:
///   - <c>Id</c>          non-blank
///   - <c>Range</c>       >= 0
///   - <c>BatteryKwh</c>  >  0
///   - <c>Brand</c>       non-blank on rename
///   - <c>LicensePlate</c> non-blank on assign
/// </summary>
public class Vehicle
{
    public string Id { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public int CalibratedRangeKm { get; private set; }
    public double BatteryCapacityKwh { get; private set; }
    public DateOnly PurchaseDate { get; private set; }
    public string LicensePlate { get; private set; } = string.Empty;

    /// <summary>
    /// EF parameterless ctor. Not for application code; only the
    /// ORM may call this. The post-init default values are
    /// placeholders that EF will overwrite from the row.
    /// </summary>
    private Vehicle() { }

    public Vehicle(
        string id,
        string brand,
        int calibratedRangeKm,
        double batteryCapacityKwh,
        DateOnly purchaseDate,
        string licensePlate)
    {
        Id = ValidateNonBlank(id, nameof(id));
        Brand = ValidateNonBlank(brand, nameof(brand));
        ValidateRange(calibratedRangeKm);
        ValidateCapacity(batteryCapacityKwh);
        PurchaseDate = purchaseDate;
        LicensePlate = ValidateNonBlank(licensePlate, nameof(licensePlate));

        CalibratedRangeKm = calibratedRangeKm;
        BatteryCapacityKwh = batteryCapacityKwh;
    }

    /// <summary>
    /// Update the brand label. Refuses blank input to keep the
    /// "non-blank" invariant.
    /// </summary>
    public void Rename(string brand) =>
        Brand = ValidateNonBlank(brand, nameof(brand));

    /// <summary>
    /// Update the vehicle's calibrated range and battery capacity
    /// in one shot. The two values are paired (range ≈
    /// battery-capacity × efficiency), so a single method keeps
    /// them in step.
    /// </summary>
    public void UpdateCapacity(int rangeKm, double capacityKwh)
    {
        ValidateRange(rangeKm);
        ValidateCapacity(capacityKwh);
        CalibratedRangeKm = rangeKm;
        BatteryCapacityKwh = capacityKwh;
    }

    /// <summary>
    /// Replace the license plate. Refuses blank input — empty
    /// plates are never valid on the wire (evGRpc enforces
    /// UNIQUE on this field, which a blank would collapse).
    /// </summary>
    public void AssignLicensePlate(string plate) =>
        LicensePlate = ValidateNonBlank(plate, nameof(plate));

    /// <summary>
    /// Current battery percent from the most recent <see cref="Chargings.Charging"/>
    /// in <paramref name="history"/>. Returns 0 when there is no
    /// history (the "unknown" sentinel). Wired up in Step 2.4
    /// along with <c>Charging</c>; today it is a placeholder.
    /// </summary>
    public int CurrentBatteryPercent(IReadOnlyList<Chargings.Charging> history)
    {
        if (history is null || history.Count == 0) return 0;
        // Caller is expected to pass history sorted by end-time;
        // we pick the largest end time to be robust to order.
        return history.Max(c => c.EndPercent);
    }

    private static string ValidateNonBlank(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{paramName} must be non-blank.", paramName)
            : value;

    private static void ValidateRange(int rangeKm)
    {
        if (rangeKm < 0)
            throw new ArgumentOutOfRangeException(nameof(rangeKm), rangeKm,
                "Calibrated range must be >= 0.");
    }

    private static void ValidateCapacity(double capacityKwh)
    {
        if (capacityKwh <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacityKwh), capacityKwh,
                "Battery capacity must be > 0 kWh.");
    }
}
