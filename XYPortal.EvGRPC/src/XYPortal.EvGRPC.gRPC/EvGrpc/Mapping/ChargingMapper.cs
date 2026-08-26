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

    public static Evgrpc.Charging ToProto(this Charging entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.Charging
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
    }

    public static Evgrpc.CreateChargingRequest ToCreateRequest(this Charging entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.CreateChargingRequest
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
    }

    // ----- timestamp helpers -----

    internal static DateTimeOffset ToDomainDto(this Timestamp ts) =>
        ts.ToDateTimeOffset();

    internal static Timestamp ToProtoTimestamp(this DateTimeOffset dto) =>
        Timestamp.FromDateTimeOffset(dto);
}
