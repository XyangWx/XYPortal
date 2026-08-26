using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.Chargings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChargingMappers : MapperBase<Charging, ChargingDto>
{
    public override partial ChargingDto Map(Charging source);

    public override partial void Map(Charging source, ChargingDto destination);
}

public static class ChargingMapperExtensions
{
    public static CreateUpdateChargingDto ToCreateInput(this Charging source) => new()
    {
        VehicleId = source.VehicleId,
        StartTime = source.StartTime,
        EndTime = source.EndTime,
        StartPercent = source.StartPercent,
        EndPercent = source.EndPercent,
        StartMileageKm = source.StartMileageKm,
        EndMileageKm = source.EndMileageKm,
        KwhCharged = source.KwhCharged,
        Cost = source.Cost,
        ElectricityUnitPrice = source.ElectricityUnitPrice,
        ServiceFee = source.ServiceFee,
        ChargerType = source.ChargerType,
        SourceCategoryId = source.SourceCategoryId,
        Location = source.Location,
        Remark = source.Remark,
    };
}
