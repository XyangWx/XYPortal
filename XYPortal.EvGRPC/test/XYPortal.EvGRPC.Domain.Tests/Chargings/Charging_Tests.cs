using System;
using Shouldly;
using Xunit;

namespace XYPortal.EvGRPC.Chargings;

public class Charging_Tests
{
    [Fact]
    public void Ctor_with_valid_args_builds_entity()
    {
        var c = new Charging(
            id: "chg-1",
            vehicleId: "veh-1",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 20,
            endPercent: 80,
            startMileageKm: 1000,
            endMileageKm: 1050,
            kwhCharged: 45.5,
            cost: 60.0,
            electricityUnitPrice: 1.2,
            serviceFee: null,
            chargerType: ChargerType.Fast,
            sourceCategoryId: "cat-home",
            location: "Beijing Chaoyang",
            remark: "test");
        c.Id.ShouldBe("chg-1");
        c.EndPercent.ShouldBe(80);
    }

    [Fact]
    public void Ctor_rejects_endPercent_lower_than_startPercent()
    {
        Should.Throw<ArgumentException>(() => new Charging(
            id: "chg-1",
            vehicleId: "veh-1",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 80,
            endPercent: 50,
            startMileageKm: 1000,
            endMileageKm: 1050,
            kwhCharged: 1,
            cost: 1,
            electricityUnitPrice: 1,
            serviceFee: null,
            chargerType: ChargerType.Fast,
            sourceCategoryId: "c",
            location: "l",
            remark: null));
    }

    [Fact]
    public void CostPerKwh_handles_null_service_fee()
    {
        var c = new Charging(
            id: "chg", vehicleId: "veh",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 0, endPercent: 100,
            startMileageKm: 0, endMileageKm: 10,
            kwhCharged: 10, cost: 12, electricityUnitPrice: 1.2,
            serviceFee: null, chargerType: ChargerType.Fast,
            sourceCategoryId: "c", location: "l", remark: null);
        c.CostPerKwh().ShouldBe(1.2);
    }

    [Fact]
    public void CostPerKwh_includes_service_fee_when_present()
    {
        var c = new Charging(
            id: "chg", vehicleId: "veh",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 0, endPercent: 100,
            startMileageKm: 0, endMileageKm: 10,
            kwhCharged: 10, cost: 12, electricityUnitPrice: 1.2,
            serviceFee: 3, chargerType: ChargerType.Fast,
            sourceCategoryId: "c", location: "l", remark: null);
        c.CostPerKwh().ShouldBe(1.5);
    }

    [Fact]
    public void Create_factory_skips_id_check()
    {
        var c = Charging.Create(
            vehicleId: "veh-1",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 20, endPercent: 80,
            startMileageKm: 1000, endMileageKm: 1050,
            kwhCharged: 30, cost: 40, electricityUnitPrice: 1,
            serviceFee: null, chargerType: ChargerType.Fast,
            sourceCategoryId: "c", location: "l", remark: null);
        c.Id.ShouldBe(string.Empty);
    }

    [Fact]
    public void Create_factory_enforces_other_invariants()
    {
        // empty vehicleId
        Should.Throw<ArgumentException>(() => Charging.Create(
            vehicleId: "",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 0, endPercent: 100,
            startMileageKm: 0, endMileageKm: 1,
            kwhCharged: 1, cost: 1, electricityUnitPrice: 1,
            serviceFee: null, chargerType: ChargerType.Fast,
            sourceCategoryId: "c", location: "l", remark: null));
        // endPercent < startPercent
        Should.Throw<ArgumentException>(() => Charging.Create(
            vehicleId: "v",
            startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
            startPercent: 80, endPercent: 50,
            startMileageKm: 0, endMileageKm: 1,
            kwhCharged: 1, cost: 1, electricityUnitPrice: 1,
            serviceFee: null, chargerType: ChargerType.Fast,
            sourceCategoryId: "c", location: "l", remark: null));
    }
}
