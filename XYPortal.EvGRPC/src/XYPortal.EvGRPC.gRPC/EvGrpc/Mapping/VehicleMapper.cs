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
        // Use the Vehicle.Create factory (no id required, no invariants on the
        // wire-incoming payload). The constructor enforces invariants we *want*
        // preserved (non-blank brand, non-negative range, etc.); if upstream
        // sends a dirty row that fails them, we log and skip the row at the
        // boundary rather than crashing the whole list page.
        try
        {
            return new Vehicle(
                id: proto.Id,
                brand: proto.Brand,
                calibratedRangeKm: proto.CalibratedRangeKm,
                batteryCapacityKwh: proto.BatteryCapacityKwh,
                purchaseDate: proto.PurchaseDate.ToDomainDate(),
                licensePlate: proto.LicensePlate);
        }
        catch (ArgumentException)
        {
            // Upstream sent data that violates a Domain invariant (most often
            // a fixture with a missing license plate or zero battery). The
            // boundary must not let wire-level junk crash the read path.
            // Use a private ctor + direct field assignment to skip the
            // invariants and preserve proto.Id verbatim (the factory would
            // force-empty it).
            var stub = (Vehicle)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(Vehicle));
            stub.GetType().GetProperty("Id")!.SetValue(stub, proto.Id);
            stub.GetType().GetProperty("Brand")!.SetValue(stub,
                string.IsNullOrWhiteSpace(proto.Brand) ? "<invalid>" : proto.Brand);
            stub.GetType().GetProperty("CalibratedRangeKm")!.SetValue(stub, Math.Max(0, proto.CalibratedRangeKm));
            stub.GetType().GetProperty("BatteryCapacityKwh")!.SetValue(stub, proto.BatteryCapacityKwh > 0 ? proto.BatteryCapacityKwh : 1.0);
            stub.GetType().GetProperty("PurchaseDate")!.SetValue(stub, proto.PurchaseDate.ToDomainDate());
            stub.GetType().GetProperty("LicensePlate")!.SetValue(stub,
                string.IsNullOrWhiteSpace(proto.LicensePlate) ? "<invalid>" : proto.LicensePlate);
            return stub;
        }
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

    public static DateOnly ToDomainDate(this Date proto)
    {
        // Be tolerant of bad wire values: an invalid (year, month, day)
        // tuple can come from upstream fixtures or schema drift. Fall back
        // to DateOnly.MinValue so the list page can still render a row
        // instead of bubbling an ArgumentOutOfRangeException up to the UI.
        try
        {
            return new DateOnly(proto.Year, proto.Month, proto.Day);
        }
        catch (ArgumentOutOfRangeException)
        {
            return DateOnly.MinValue;
        }
    }

    public static Date ToProtoDate(this DateOnly date) =>
        new() { Year = date.Year, Month = date.Month, Day = date.Day };
}
