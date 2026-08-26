using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Vehicles;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class VehicleMappers : MapperBase<Vehicle, VehicleDto>
{
    public override partial VehicleDto Map(Vehicle source);

    public override partial void Map(Vehicle source, VehicleDto destination);
}

public static class VehicleMapperExtensions
{
    /// <summary>
    /// Convert a Domain <see cref="Vehicle"/> into the input DTO
    /// used for create / update RPC calls. The input does not carry
    /// the server-assigned <c>Id</c>.
    /// </summary>
    public static CreateUpdateVehicleDto ToCreateInput(this Vehicle source) => new()
    {
        Brand = source.Brand,
        CalibratedRangeKm = source.CalibratedRangeKm,
        BatteryCapacityKwh = source.BatteryCapacityKwh,
        PurchaseDate = source.PurchaseDate,
        LicensePlate = source.LicensePlate,
    };
}
