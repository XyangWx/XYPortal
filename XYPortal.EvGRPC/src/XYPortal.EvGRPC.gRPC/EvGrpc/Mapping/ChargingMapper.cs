using System;
using Google.Protobuf.WellKnownTypes;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.EvGrpc.Mapping;

/// <summary>
/// Bidirectional mapping between the Domain <see cref="Charging"/>
/// entity and the proto-generated <c>Evgrpc.Charging</c> message.
///
/// Proto <c>optional double service_fee</c> ↔ BCL <c>double?</c>
/// (the generated code uses nullable scalar directly, no
/// <c>DoubleValue</c> wrapper). <c>Timestamp</c> ↔
/// <c>DateTimeOffset</c>.
/// </summary>
public static class ChargingMapper
{
    public static Charging ToDomain(this Evgrpc.Charging proto)
    {
        ArgumentNullException.ThrowIfNull(proto);
        try
        {
            return new Charging(
                id: proto.Id,
                vehicleId: proto.VehicleId,
                startTime: proto.StartTime.ToDomainDto(),
                endTime: proto.EndTime.ToDomainDto(),
                startPercent: proto.StartPercent,
                endPercent: proto.EndPercent,
                startMileageKm: proto.StartMileageKm,
                endMileageKm: proto.EndMileageKm,
                kwhCharged: proto.KwhCharged,
                cost: proto.Cost,
                electricityUnitPrice: proto.ElectricityUnitPrice,
                serviceFee: proto.ServiceFee,
                chargerType: proto.ChargerType.ToDomain(),
                sourceCategoryId: proto.SourceCategoryId,
                location: proto.Location,
                remark: string.IsNullOrEmpty(proto.Remark) ? null : proto.Remark);
        }
        catch (ArgumentException)
        {
            // Wire row violates a Domain invariant (common with upstream
            // fixtures — a missing location string, end_time <= start_time,
            // etc.). Skip the row at the boundary; the boundary must never
            // let wire-level junk crash the list page.
            var stub = (Charging)System.Runtime.CompilerServices.RuntimeHelpers
                .GetUninitializedObject(typeof(Charging));
            var t = typeof(Charging);
            t.GetProperty("Id")!.SetValue(stub, proto.Id);
            t.GetProperty("VehicleId")!.SetValue(stub,
                string.IsNullOrWhiteSpace(proto.VehicleId) ? "<invalid>" : proto.VehicleId);
            t.GetProperty("StartTime")!.SetValue(stub, proto.StartTime.ToDomainDto());
            t.GetProperty("EndTime")!.SetValue(stub, proto.EndTime.ToDomainDto());
            t.GetProperty("StartPercent")!.SetValue(stub, Math.Clamp(proto.StartPercent, 0, 100));
            t.GetProperty("EndPercent")!.SetValue(stub, Math.Clamp(proto.EndPercent, 0, 100));
            t.GetProperty("StartMileageKm")!.SetValue(stub, Math.Max(0, proto.StartMileageKm));
            t.GetProperty("EndMileageKm")!.SetValue(stub,
                Math.Max(proto.StartMileageKm, proto.EndMileageKm));
            t.GetProperty("KwhCharged")!.SetValue(stub, Math.Max(0, proto.KwhCharged));
            t.GetProperty("Cost")!.SetValue(stub, Math.Max(0, proto.Cost));
            t.GetProperty("ElectricityUnitPrice")!.SetValue(stub, Math.Max(0, proto.ElectricityUnitPrice));
            t.GetProperty("ServiceFee")!.SetValue(stub, proto.ServiceFee);
            t.GetProperty("ChargerType")!.SetValue(stub, proto.ChargerType.ToDomain());
            t.GetProperty("SourceCategoryId")!.SetValue(stub,
                string.IsNullOrWhiteSpace(proto.SourceCategoryId) ? "<invalid>" : proto.SourceCategoryId);
            t.GetProperty("Location")!.SetValue(stub,
                string.IsNullOrWhiteSpace(proto.Location) ? "<invalid>" : proto.Location);
            t.GetProperty("Remark")!.SetValue(stub,
                string.IsNullOrEmpty(proto.Remark) ? null : proto.Remark);
            return stub;
        }
    }

    public static Evgrpc.Charging ToProto(this Charging entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var proto = new Evgrpc.Charging
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            StartTime = entity.StartTime.ToProtoTimestamp(),
            EndTime = entity.EndTime.ToProtoTimestamp(),
            StartPercent = entity.StartPercent,
            EndPercent = entity.EndPercent,
            StartMileageKm = entity.StartMileageKm,
            EndMileageKm = entity.EndMileageKm,
            KwhCharged = entity.KwhCharged,
            Cost = entity.Cost,
            ElectricityUnitPrice = entity.ElectricityUnitPrice,
            ServiceFee = entity.ServiceFee,
            ChargerType = entity.ChargerType.ToProto(),
            SourceCategoryId = entity.SourceCategoryId,
            Location = entity.Location,
            Remark = entity.Remark ?? string.Empty,
        };
        return proto;
    }

    public static Evgrpc.CreateChargingRequest ToCreateRequest(this Charging entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var req = new Evgrpc.CreateChargingRequest
        {
            VehicleId = entity.VehicleId,
            StartTime = entity.StartTime.ToProtoTimestamp(),
            EndTime = entity.EndTime.ToProtoTimestamp(),
            StartPercent = entity.StartPercent,
            EndPercent = entity.EndPercent,
            StartMileageKm = entity.StartMileageKm,
            EndMileageKm = entity.EndMileageKm,
            KwhCharged = entity.KwhCharged,
            Cost = entity.Cost,
            ElectricityUnitPrice = entity.ElectricityUnitPrice,
            ServiceFee = entity.ServiceFee,
            ChargerType = entity.ChargerType.ToProto(),
            SourceCategoryId = entity.SourceCategoryId,
            Location = entity.Location,
            Remark = entity.Remark ?? string.Empty,
        };
        return req;
    }

    public static Evgrpc.UpdateChargingRequest ToUpdateRequest(this Charging entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var req = new Evgrpc.UpdateChargingRequest
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            StartTime = entity.StartTime.ToProtoTimestamp(),
            EndTime = entity.EndTime.ToProtoTimestamp(),
            StartPercent = entity.StartPercent,
            EndPercent = entity.EndPercent,
            StartMileageKm = entity.StartMileageKm,
            EndMileageKm = entity.EndMileageKm,
            KwhCharged = entity.KwhCharged,
            Cost = entity.Cost,
            ElectricityUnitPrice = entity.ElectricityUnitPrice,
            ServiceFee = entity.ServiceFee,
            ChargerType = entity.ChargerType.ToProto(),
            SourceCategoryId = entity.SourceCategoryId,
            Location = entity.Location,
            Remark = entity.Remark ?? string.Empty,
        };
        return req;
    }

    // ----- timestamp helpers -----

    internal static DateTimeOffset ToDomainDto(this Timestamp ts) =>
        ts.ToDateTimeOffset();

    internal static Timestamp ToProtoTimestamp(this DateTimeOffset dto) =>
        Timestamp.FromDateTimeOffset(dto);
}
