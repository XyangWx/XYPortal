using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.Vehicles;

/// <summary>
/// Pure-domain tests for <see cref="Vehicle"/>. No ABP DI, no DB,
/// no gRPC — just construct the entity and exercise behavior. This
/// is the value of the 充血 model: every invariant lives next to
/// the data and can be verified without a container.
/// </summary>
public class Vehicle_Tests
{
    private const string DefaultId = "veh-1";
    private const string DefaultBrand = "Tesla";
    private const int DefaultRange = 500;
    private const double DefaultBattery = 75.0;
    private static readonly DateOnly DefaultPurchaseDate = new(2024, 1, 15);
    private const string DefaultPlate = "京A12345";

    private static Vehicle NewValidVehicle() => new(
        id: DefaultId,
        brand: DefaultBrand,
        calibratedRangeKm: DefaultRange,
        batteryCapacityKwh: DefaultBattery,
        purchaseDate: DefaultPurchaseDate,
        licensePlate: DefaultPlate);

    [Fact]
    public void Rename_with_new_brand_updates_Brand()
    {
        var v = NewValidVehicle();
        v.Rename("BYD");
        v.Brand.ShouldBe("BYD");
    }

    [Fact]
    public void Rename_rejects_blank_brand()
    {
        var v = NewValidVehicle();
        Should.Throw<ArgumentException>(() => v.Rename(""));
    }

    [Fact]
    public void Constructor_rejects_blank_id()
    {
        Should.Throw<ArgumentException>(() => new Vehicle(
            id: "",
            brand: DefaultBrand,
            calibratedRangeKm: DefaultRange,
            batteryCapacityKwh: DefaultBattery,
            purchaseDate: DefaultPurchaseDate,
            licensePlate: DefaultPlate));
    }

    [Fact]
    public void Constructor_rejects_negative_range()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Vehicle(
            id: DefaultId,
            brand: DefaultBrand,
            calibratedRangeKm: -1,
            batteryCapacityKwh: DefaultBattery,
            purchaseDate: DefaultPurchaseDate,
            licensePlate: DefaultPlate));
    }

    [Fact]
    public void Constructor_rejects_non_positive_battery_capacity()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new Vehicle(
            id: DefaultId,
            brand: DefaultBrand,
            calibratedRangeKm: DefaultRange,
            batteryCapacityKwh: 0.0,
            purchaseDate: DefaultPurchaseDate,
            licensePlate: DefaultPlate));
    }

    [Fact]
    public void UpdateCapacity_replaces_range_and_capacity()
    {
        var v = NewValidVehicle();
        v.UpdateCapacity(rangeKm: 600, capacityKwh: 80.0);
        v.CalibratedRangeKm.ShouldBe(600);
        v.BatteryCapacityKwh.ShouldBe(80.0);
    }

    [Fact]
    public void UpdateCapacity_rejects_invalid_values()
    {
        var v = NewValidVehicle();
        Should.Throw<ArgumentOutOfRangeException>(() => v.UpdateCapacity(-1, 80.0));
        Should.Throw<ArgumentOutOfRangeException>(() => v.UpdateCapacity(500, 0.0));
    }

    [Fact]
    public void AssignLicensePlate_replaces_value()
    {
        var v = NewValidVehicle();
        v.AssignLicensePlate("京B99999");
        v.LicensePlate.ShouldBe("京B99999");
    }

    [Fact]
    public void AssignLicensePlate_rejects_blank()
    {
        var v = NewValidVehicle();
        Should.Throw<ArgumentException>(() => v.AssignLicensePlate(""));
    }

    [Fact]
    public void CurrentBatteryPercent_returns_zero_with_no_history()
    {
        var v = NewValidVehicle();
        v.CurrentBatteryPercent(new List<Charging>()).ShouldBe(0);
        v.CurrentBatteryPercent(Array.Empty<Charging>()).ShouldBe(0);
    }

    [Fact]
    public void CurrentBatteryPercent_returns_max_EndPercent_in_history()
    {
        var v = NewValidVehicle();
        var history = new List<Charging>
        {
            MakeCharging(endPercent: 60),
            MakeCharging(endPercent: 90),
            MakeCharging(endPercent: 75),
        };
        v.CurrentBatteryPercent(history).ShouldBe(90);
    }

    private static Charging MakeCharging(int endPercent) => new(
        id: $"chg-{endPercent}",
        vehicleId: DefaultId,
        startTime: new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
        endTime: new DateTimeOffset(2024, 1, 15, 11, 0, 0, TimeSpan.Zero),
        startPercent: 20,
        endPercent: endPercent,
        startMileageKm: 1000,
        endMileageKm: 1050,
        kwhCharged: 30.0,
        cost: 50.0,
        electricityUnitPrice: 1.2,
        serviceFee: null,
        chargerType: ChargerType.Fast,
        sourceCategoryId: "cat-1",
        location: "Beijing Chaoyang",
        remark: null);
}
