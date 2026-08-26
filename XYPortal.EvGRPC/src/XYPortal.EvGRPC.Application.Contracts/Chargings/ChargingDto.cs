using System;

namespace XYPortal.EvGRPC.Chargings;

public class ChargingDto
{
    public string Id { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int StartPercent { get; set; }
    public int EndPercent { get; set; }
    public int StartMileageKm { get; set; }
    public int EndMileageKm { get; set; }
    public double KwhCharged { get; set; }
    public double Cost { get; set; }
    public double ElectricityUnitPrice { get; set; }
    public double? ServiceFee { get; set; }
    public ChargerType ChargerType { get; set; }
    public string SourceCategoryId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Remark { get; set; }
}
