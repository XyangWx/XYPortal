using System;

namespace XYPortal.EvGRPC.Chargings;

/// <summary>
/// Aggregate: a single charging session for a
/// <see cref="Vehicles.Vehicle"/>. The 充血 contract enforces
/// invariants on <c>StartPercent &lt;= EndPercent</c>,
/// non-negative mileage delta, and that the period is well-formed
/// (see <see cref="ChargingPeriod"/>).
///
/// This is a value-bearing entity (not just a row): the charging
/// report KPIs (cost per kWh, effective duration) live as behavior
/// methods here so Application layer code does not re-derive them.
/// </summary>
public class Charging
{
    public string Id { get; private set; } = string.Empty;
    public string VehicleId { get; private set; } = string.Empty;
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public int StartPercent { get; private set; }
    public int EndPercent { get; private set; }
    public int StartMileageKm { get; private set; }
    public int EndMileageKm { get; private set; }
    public double KwhCharged { get; private set; }
    public double Cost { get; private set; }
    public double ElectricityUnitPrice { get; private set; }
    public double? ServiceFee { get; private set; }
    public ChargerType ChargerType { get; private set; }
    public string SourceCategoryId { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string? Remark { get; private set; }

    private Charging() { }

    public Charging(
        string id,
        string vehicleId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int startPercent,
        int endPercent,
        int startMileageKm,
        int endMileageKm,
        double kwhCharged,
        double cost,
        double electricityUnitPrice,
        double? serviceFee,
        ChargerType chargerType,
        string sourceCategoryId,
        string location,
        string? remark)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("vehicleId must be non-blank.", nameof(vehicleId));
        // ChargingPeriod's ctor enforces end > start.
        _ = new ChargingPeriod(startTime, endTime);

        ValidatePercent(startPercent, nameof(startPercent));
        ValidatePercent(endPercent, nameof(endPercent));
        if (endPercent < startPercent)
            throw new ArgumentException(
                $"EndPercent ({endPercent}) must be >= StartPercent ({startPercent}).");
        if (startMileageKm < 0)
            throw new ArgumentOutOfRangeException(nameof(startMileageKm));
        if (endMileageKm < startMileageKm)
            throw new ArgumentOutOfRangeException(nameof(endMileageKm),
                "EndMileageKm must be >= StartMileageKm.");
        if (kwhCharged < 0)
            throw new ArgumentOutOfRangeException(nameof(kwhCharged));
        if (cost < 0)
            throw new ArgumentOutOfRangeException(nameof(cost));
        if (electricityUnitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(electricityUnitPrice));
        if (string.IsNullOrWhiteSpace(sourceCategoryId))
            throw new ArgumentException("sourceCategoryId must be non-blank.", nameof(sourceCategoryId));
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("location must be non-blank.", nameof(location));

        Id = id;
        VehicleId = vehicleId;
        StartTime = startTime;
        EndTime = endTime;
        StartPercent = startPercent;
        EndPercent = endPercent;
        StartMileageKm = startMileageKm;
        EndMileageKm = endMileageKm;
        KwhCharged = kwhCharged;
        Cost = cost;
        ElectricityUnitPrice = electricityUnitPrice;
        ServiceFee = serviceFee;
        ChargerType = chargerType;
        SourceCategoryId = sourceCategoryId;
        Location = location;
        Remark = remark;
    }

    /// <summary>
    /// KWh charged, rounded to two decimals (matches the precision
    /// shown in evGRpc's reporting UI; downstream consumers like
    /// Razor don't need 15-digit precision).
    /// </summary>
    public double MeasuredKwh() => Math.Round(KwhCharged, 2);

    /// <summary>Total wall-clock duration of the session.</summary>
    public TimeSpan Duration() => EndTime - StartTime;

    /// <summary>
    /// Effective price per kWh including service fee, or
    /// <c>ElectricityUnitPrice</c> alone when no service fee is
    /// recorded (proto uses <c>google.protobuf.DoubleValue</c>
    /// for nullability).
    /// </summary>
    public double CostPerKwh()
    {
        if (KwhCharged <= 0) return 0;
        var total = ServiceFee.HasValue ? Cost + ServiceFee.Value : Cost;
        return Math.Round(total / KwhCharged, 4);
    }

    /// <summary>True when this is a fast-charger session.</summary>
    public bool IsFastCharge() => ChargerType == ChargerType.Fast;

    private static void ValidatePercent(int pct, string paramName)
    {
        if (pct < 0 || pct > 100)
            throw new ArgumentOutOfRangeException(paramName, pct,
                "Battery percent must be in [0, 100].");
    }
}
