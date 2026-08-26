using System;

namespace XYPortal.EvGRPC.Vehicles;

public class VehicleDto
{
    public string Id { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int CalibratedRangeKm { get; set; }
    public double BatteryCapacityKwh { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
}
