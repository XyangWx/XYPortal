using System;
using Google.Protobuf.WellKnownTypes;
using Google.Type;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.EvGrpc.Mapping;

/// <summary>
/// Bidirectional mapping between the Domain <see cref="Vehicle"/>
/// aggregate and the proto-generated <c>Evgrpc.Vehicle</c> message.
///
/// Lives in the gRPC project (not Domain) so the Domain layer
/// stays free of any proto / WellKnownTypes dependency.
///
/// Date conversion: BCL <c>DateOnly</c> ↔ <c>Google.Type.Date</c>.
/// </summary>
public static class VehicleMapper
{
    public static Vehicle ToDomain(this Evgrpc.Vehicle proto)
    {
        ArgumentNullException.ThrowIfNull(proto);
        return new Vehicle(
            id: proto.Id,
            brand: proto.Brand,
            calibratedRangeKm: proto.CalibratedRangeKm,
            batteryCapacityKwh: proto.BatteryCapacityKwh,
            purchaseDate: proto.PurchaseDate.ToDomainDate(),
            licensePlate: proto.LicensePlate);
    }

    public static Evgrpc.Vehicle ToProto(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.Vehicle
        {
            Id = entity.Id,
            Brand = entity.Brand,
            CalibratedRangeKm = entity.CalibratedRangeKm,
            BatteryCapacityKwh = entity.BatteryCapacityKwh,
            PurchaseDate = entity.PurchaseDate.ToProtoDate(),
            LicensePlate = entity.LicensePlate,
        };
    }

    public static Evgrpc.CreateVehicleRequest ToCreateRequest(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.CreateVehicleRequest
        {
            Brand = entity.Brand,
            CalibratedRangeKm = entity.CalibratedRangeKm,
            BatteryCapacityKwh = entity.BatteryCapacityKwh,
            PurchaseDate = entity.PurchaseDate.ToProtoDate(),
            LicensePlate = entity.LicensePlate,
        };
    }

    public static Evgrpc.UpdateVehicleRequest ToUpdateRequest(this Vehicle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.UpdateVehicleRequest
        {
            Id = entity.Id,
            Brand = entity.Brand,
            CalibratedRangeKm = entity.CalibratedRangeKm,
            BatteryCapacityKwh = entity.BatteryCapacityKwh,
            PurchaseDate = entity.PurchaseDate.ToProtoDate(),
            LicensePlate = entity.LicensePlate,
        };
    }

    // ----- date helpers -----

    public static DateOnly ToDomainDate(this Date proto) =>
        new(proto.Year, proto.Month, proto.Day);

    public static Date ToProtoDate(this DateOnly date) =>
        new() { Year = date.Year, Month = date.Month, Day = date.Day };
}
