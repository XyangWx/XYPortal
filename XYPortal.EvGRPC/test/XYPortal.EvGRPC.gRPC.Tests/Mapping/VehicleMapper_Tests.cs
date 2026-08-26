using System;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;
using XYPortal.EvGRPC.EvGrpc.Mapping;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Mapping;

/// <summary>
/// Round-trip tests: build a Domain entity, convert to proto,
/// convert back, verify equality. These are the verify step from
/// plan-evgrpc.md §2.3 and they also lock down the wire contract:
/// any future change to the mapper or the proto fields breaks them.
/// </summary>
public class VehicleMapper_Tests
{
    private static Vehicle NewDomainVehicle() => new(
        id: "veh-1",
        brand: "Tesla",
        calibratedRangeKm: 500,
        batteryCapacityKwh: 75.5,
        purchaseDate: new DateOnly(2024, 1, 15),
        licensePlate: "京A12345");

    [Fact]
    public void ToDomain_produces_equal_entity()
    {
        var domain = NewDomainVehicle();
        var proto = domain.ToProto();
        var roundTrip = proto.ToDomain();

        roundTrip.Id.ShouldBe(domain.Id);
        roundTrip.Brand.ShouldBe(domain.Brand);
        roundTrip.CalibratedRangeKm.ShouldBe(domain.CalibratedRangeKm);
        roundTrip.BatteryCapacityKwh.ShouldBe(domain.BatteryCapacityKwh);
        roundTrip.PurchaseDate.ShouldBe(domain.PurchaseDate);
        roundTrip.LicensePlate.ShouldBe(domain.LicensePlate);
    }

    [Fact]
    public void ToProto_roundtrips_via_ToDomain()
    {
        var domain = NewDomainVehicle();
        var proto = domain.ToProto();
        var roundTrip = proto.ToDomain().ToProto();

        // Compare the proto messages directly: Equals is implemented
        // by the protoc-generated code.
        roundTrip.Equals(proto).ShouldBeTrue();
    }

    [Fact]
    public void ToCreateRequest_carries_other_fields()
    {
        var domain = NewDomainVehicle();
        var req = domain.ToCreateRequest();

        req.Brand.ShouldBe(domain.Brand);
        req.CalibratedRangeKm.ShouldBe(domain.CalibratedRangeKm);
        req.BatteryCapacityKwh.ShouldBe(domain.BatteryCapacityKwh);
        req.LicensePlate.ShouldBe(domain.LicensePlate);
        req.PurchaseDate.Year.ShouldBe(domain.PurchaseDate.Year);
        req.PurchaseDate.Month.ShouldBe(domain.PurchaseDate.Month);
        req.PurchaseDate.Day.ShouldBe(domain.PurchaseDate.Day);
    }

    [Fact]
    public void GoogleTypeDate_conversion_preserves_calendar_date()
    {
        var date = new DateOnly(2026, 12, 31);
        var proto = date.ToProtoDate();
        proto.Year.ShouldBe(2026);
        proto.Month.ShouldBe(12);
        proto.Day.ShouldBe(31);
        proto.ToDomainDate().ShouldBe(date);
    }
}
