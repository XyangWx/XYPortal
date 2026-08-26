using System;
using System.ComponentModel.DataAnnotations;

namespace XYPortal.EvGRPC.Vehicles;

public class CreateUpdateVehicleDto
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string Brand { get; set; } = string.Empty;

    [Range(0, 10000)]
    public int CalibratedRangeKm { get; set; }

    [Range(0.1, 1000.0)]
    public double BatteryCapacityKwh { get; set; }

    public DateOnly PurchaseDate { get; set; }

    [Required]
    [StringLength(16, MinimumLength = 1)]
    public string LicensePlate { get; set; } = string.Empty;
}
