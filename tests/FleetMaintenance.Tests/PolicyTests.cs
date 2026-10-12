using System;
using System.Globalization;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class PolicyTests
{
    private static readonly DateOnly Today = Fixture.Today;
    private readonly MaintenancePolicy _policy = new();

    public static TheoryData<VehicleType, ServiceKind> Combinations()
    {
        var data = new TheoryData<VehicleType, ServiceKind>();
        foreach (VehicleType type in Enum.GetValues<VehicleType>())
        {
            foreach (ServiceKind kind in Enum.GetValues<ServiceKind>())
            {
                data.Add(type, kind);
            }
        }

        return data;
    }

    [Theory]
    [InlineData(0, 10000, MaintenanceStatus.Ok)]
    [InlineData(7999, 10000, MaintenanceStatus.Ok)]
    [InlineData(8000, 10000, MaintenanceStatus.DueSoon)]
    [InlineData(10000, 10000, MaintenanceStatus.DueSoon)]
    [InlineData(10001, 10000, MaintenanceStatus.Overdue)]
    [InlineData(9, 12, MaintenanceStatus.Ok)]
    [InlineData(10, 12, MaintenanceStatus.DueSoon)]
    [InlineData(12, 12, MaintenanceStatus.DueSoon)]
    [InlineData(13, 12, MaintenanceStatus.Overdue)]
    public void Classify_РядкиТаблиціРішень_ПовертаєОчікуванийСтан(
        int elapsed,
        int limit,
        MaintenanceStatus expected)
    {
        MaintenanceStatus actual = MaintenancePolicy.Classify(elapsed, limit);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("2026-01-15", "2026-02-14", 0)]
    [InlineData("2026-01-15", "2026-02-15", 1)]
    [InlineData("2025-11-02", "2026-11-02", 12)]
    [InlineData("2026-01-31", "2026-02-28", 0)]
    [InlineData("2026-02-01", "2026-01-01", -1)]
    public void MonthsBetween_РізніДати_РахуєЛишеПовніМісяці(string origin, string to, int expected)
    {
        DateOnly start = DateOnly.Parse(origin, CultureInfo.InvariantCulture);
        DateOnly end = DateOnly.Parse(to, CultureInfo.InvariantCulture);

        int months = MaintenancePolicy.MonthsBetween(start, end);

        Assert.Equal(expected, months);
    }

    [Theory]
    [InlineData(7999, MaintenanceStatus.Ok)]
    [InlineData(8000, MaintenanceStatus.DueSoon)]
    [InlineData(10000, MaintenanceStatus.DueSoon)]
    [InlineData(10001, MaintenanceStatus.Overdue)]
    public void Evaluate_ЛегковаМастило_ЗаПробігом(int mileage, MaintenanceStatus expected)
    {
        Vehicle car = Fixture.Make(1, Info(VehicleType.Car), new DateOnly(2026, 10, 1), mileage);

        MaintenanceCheck check = _policy.Evaluate(car, ServiceKind.OilChange, Today);

        Assert.Equal(expected, check.Status);
        Assert.Equal(mileage, check.KmSince);
    }

    [Theory]
    [InlineData(2026, 3, 2, MaintenanceStatus.Ok)]
    [InlineData(2026, 2, 2, MaintenanceStatus.Ok)]
    [InlineData(2026, 1, 2, MaintenanceStatus.DueSoon)]
    [InlineData(2025, 11, 2, MaintenanceStatus.DueSoon)]
    [InlineData(2025, 10, 2, MaintenanceStatus.Overdue)]
    public void Evaluate_ЛегковаМастило_ЗаДатою(int year, int month, int day, MaintenanceStatus expected)
    {
        var registered = new DateOnly(year, month, day);
        Vehicle car = Fixture.Make(1, Info(VehicleType.Car), registered, 0);

        MaintenanceCheck check = _policy.Evaluate(car, ServiceKind.OilChange, Today);

        Assert.Equal(expected, check.Status);
    }

    [Theory]
    [InlineData(11000, 2026, 10, 1)]
    [InlineData(100, 2025, 1, 1)]
    public void Evaluate_ОднаОзнакаПрострочена_ПідсумокПрострочено(int km, int y, int m, int d)
    {
        Vehicle car = Fixture.Make(1, Info(VehicleType.Car), new DateOnly(y, m, d), km);

        MaintenanceCheck check = _policy.Evaluate(car, ServiceKind.OilChange, Today);

        Assert.Equal(MaintenanceStatus.Overdue, check.Status);
    }

    [Fact]
    public void Evaluate_ЄОстаннєТО_ВідліковІзНьогоАНеЗРеєстрації()
    {
        Vehicle car = Fixture.Make(1, Info(VehicleType.Car), new DateOnly(2020, 1, 1), 100000);
        var point = new ServicePoint(new DateOnly(2026, 10, 1), 95000);
        car.AddServiceRecord(new ServiceRecord(ServiceKind.OilChange, point));

        MaintenanceCheck check = _policy.Evaluate(car, ServiceKind.OilChange, Today);

        Assert.Equal(MaintenanceStatus.Ok, check.Status);
        Assert.Equal(5000, check.KmSince);
        Assert.Equal(1, check.MonthsSince);
    }

    [Fact]
    public void Evaluate_ТОІншогоВиду_НеСкидаєЛічильник()
    {
        Vehicle car = Fixture.Make(1, Info(VehicleType.Car), new DateOnly(2024, 1, 1), 50000);
        var point = new ServicePoint(new DateOnly(2026, 10, 1), 50000);
        car.AddServiceRecord(new ServiceRecord(ServiceKind.Brakes, point));

        MaintenanceCheck check = _policy.Evaluate(car, ServiceKind.OilChange, Today);

        Assert.Equal(MaintenanceStatus.Overdue, check.Status);
    }

    [Theory]
    [InlineData(2026, 5, 2, MaintenanceStatus.DueSoon)]
    [InlineData(2026, 4, 2, MaintenanceStatus.Overdue)]
    public void Evaluate_ВантажівкаМастило_ІнтервалШістьМісяців(
        int year,
        int month,
        int day,
        MaintenanceStatus expected)
    {
        Vehicle truck = Fixture.Make(1, Info(VehicleType.Truck), new DateOnly(year, month, day), 0);

        MaintenanceCheck check = _policy.Evaluate(truck, ServiceKind.OilChange, Today);

        Assert.Equal(expected, check.Status);
    }

    [Theory]
    [MemberData(nameof(Combinations))]
    public void IntervalTable_УсіКомбінації_МаютьДодатніІнтервали(VehicleType type, ServiceKind kind)
    {
        ServiceInterval interval = IntervalTable.For(type, kind);

        Assert.True(interval.Km > 0);
        Assert.True(interval.Months > 0);
    }

    [Theory]
    [InlineData(VehicleType.Car, ServiceKind.OilChange, MaintenanceStatus.Ok, 1200)]
    [InlineData(VehicleType.Car, ServiceKind.OilChange, MaintenanceStatus.DueSoon, 1200)]
    [InlineData(VehicleType.Car, ServiceKind.OilChange, MaintenanceStatus.Overdue, 1380)]
    [InlineData(VehicleType.Car, ServiceKind.Brakes, MaintenanceStatus.Overdue, 4025)]
    [InlineData(VehicleType.Van, ServiceKind.Inspection, MaintenanceStatus.Ok, 2250)]
    [InlineData(VehicleType.Van, ServiceKind.OilChange, MaintenanceStatus.Overdue, 1725)]
    [InlineData(VehicleType.Truck, ServiceKind.Brakes, MaintenanceStatus.DueSoon, 5600)]
    [InlineData(VehicleType.Truck, ServiceKind.Brakes, MaintenanceStatus.Overdue, 6440)]
    public void Price_РядкиТаблиціЦін_ПовертаєОчікувануВартість(
        VehicleType type,
        ServiceKind kind,
        MaintenanceStatus status,
        int expected)
    {
        decimal price = new PricingPolicy().Price(type, kind, status);

        Assert.Equal((decimal)expected, price);
    }

    [Fact]
    public void Assess_ПрострочаВанМастило_ПоєднуєСтанІВартість()
    {
        var advisor = new ServiceAdvisor(_policy, new PricingPolicy());
        Vehicle van = Fixture.Make(1, Info(VehicleType.Van), new DateOnly(2026, 10, 1), 13000);

        ServiceAssessment result = advisor.Assess(van, ServiceKind.OilChange, Today);

        Assert.Equal(MaintenanceStatus.Overdue, result.Check.Status);
        Assert.Equal(1725m, result.Cost);
    }

    private static VehicleInfo Info(VehicleType type)
    {
        return new VehicleInfo("AA1111BB", "Test", type);
    }
}
