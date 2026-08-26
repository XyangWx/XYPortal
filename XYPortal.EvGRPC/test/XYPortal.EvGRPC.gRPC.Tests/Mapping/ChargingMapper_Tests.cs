using System;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.EvGrpc.Mapping;

namespace XYPortal.EvGRPC.Mapping;

public class ChargingMapper_Tests
{
    private static Charging NewDomainCharging(double? serviceFee = 5.0, string? remark = "test") =>
        new(
            id: "chg-1",
            vehicleId: "veh-1",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 30, 0, TimeSpan.Zero),
            startPercent: 20,
            endPercent: 80,
            startMileageKm: 1000,
            endMileageKm: 1050,
            kwhCharged: 45.5,
            cost: 60.0,
            electricityUnitPrice: 1.2,
            serviceFee: serviceFee,
            chargerType: ChargerType.Fast,
            sourceCategoryId: "cat-home",
            location: "Beijing Chaoyang",
            remark: remark);

    [Fact]
    public void ToDomain_produces_equal_entity()
    {
        var domain = NewDomainCharging();
        var proto = domain.ToProto();
        var roundTrip = proto.ToDomain();

        roundTrip.Id.ShouldBe(domain.Id);
        roundTrip.VehicleId.ShouldBe(domain.VehicleId);
        roundTrip.StartTime.ShouldBe(domain.StartTime);
        roundTrip.EndTime.ShouldBe(domain.EndTime);
        roundTrip.StartPercent.ShouldBe(domain.StartPercent);
        roundTrip.EndPercent.ShouldBe(domain.EndPercent);
        roundTrip.StartMileageKm.ShouldBe(domain.StartMileageKm);
        roundTrip.EndMileageKm.ShouldBe(domain.EndMileageKm);
        roundTrip.KwhCharged.ShouldBe(domain.KwhCharged);
        roundTrip.Cost.ShouldBe(domain.Cost);
        roundTrip.ElectricityUnitPrice.ShouldBe(domain.ElectricityUnitPrice);
        roundTrip.ServiceFee.ShouldBe(domain.ServiceFee);
        roundTrip.ChargerType.ShouldBe(domain.ChargerType);
        roundTrip.SourceCategoryId.ShouldBe(domain.SourceCategoryId);
        roundTrip.Location.ShouldBe(domain.Location);
        roundTrip.Remark.ShouldBe(domain.Remark);
    }

    [Fact]
    public void ToProto_roundtrips_via_ToDomain()
    {
        var domain = NewDomainCharging();
        var proto = domain.ToProto();
        var roundTrip = proto.ToDomain().ToProto();
        roundTrip.Equals(proto).ShouldBeTrue();
    }

    [Fact]
    public void ToDomain_handles_null_service_fee()
    {
        var domain = NewDomainCharging(serviceFee: null);
        var proto = domain.ToProto();
        proto.ServiceFee.ShouldBeNull();
        proto.ToDomain().ServiceFee.ShouldBeNull();
    }

    [Fact]
    public void ToDomain_handles_null_remark()
    {
        var domain = NewDomainCharging(remark: null);
        var proto = domain.ToProto();
        proto.Remark.ShouldBe(string.Empty);
        proto.ToDomain().Remark.ShouldBeNull();
    }

    [Fact]
    public void ChargerType_roundtrips_for_each_value()
    {
        foreach (var ct in new[] { ChargerType.Unspecified, ChargerType.Fast, ChargerType.Slow })
        {
            var domain = new Charging(
                id: "chg", vehicleId: "veh",
                startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
                endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
                startPercent: 0, endPercent: 100,
                startMileageKm: 0, endMileageKm: 10,
                kwhCharged: 1, cost: 1, electricityUnitPrice: 1,
                serviceFee: null, chargerType: ct,
                sourceCategoryId: "c", location: "l", remark: null);
            var proto = domain.ToProto();
            proto.ChargerType.ShouldBe(ct.ToProto());
            proto.ToDomain().ChargerType.ShouldBe(ct);
        }
    }
}
