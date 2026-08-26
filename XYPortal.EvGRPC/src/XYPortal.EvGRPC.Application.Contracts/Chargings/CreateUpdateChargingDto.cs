using System;
using System.ComponentModel.DataAnnotations;

namespace XYPortal.EvGRPC.Chargings;

public class CreateUpdateChargingDto
{
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string VehicleId { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    [Range(0, 100)]
    public int StartPercent { get; set; }

    [Range(0, 100)]
    public int EndPercent { get; set; }

    [Range(0, int.MaxValue)]
    public int StartMileageKm { get; set; }

    [Range(0, int.MaxValue)]
    public int EndMileageKm { get; set; }

    [Range(0.0, double.MaxValue)]
    public double KwhCharged { get; set; }

    [Range(0.0, double.MaxValue)]
    public double Cost { get; set; }

    [Range(0.0, double.MaxValue)]
    public double ElectricityUnitPrice { get; set; }

    public double? ServiceFee { get; set; }

    public ChargerType ChargerType { get; set; }

    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string SourceCategoryId { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Location { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Remark { get; set; }
}
