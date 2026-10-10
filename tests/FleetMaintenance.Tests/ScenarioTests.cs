using System;
using System.Linq;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class ScenarioTests
{
    private static readonly DateOnly Recent = new(2026, 10, 1);
    private readonly Fixture _f = new();

    [Fact]
    public void UC01_КоректнийПробіг_ОновлюєТЗІЗберігає()
    {
        Vehicle car = _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 1000);

        Result<Vehicle> result = _f.MileageService().Register("aa1111bb", "1500");

        Assert.True(result.IsSuccess);
        Assert.Equal(1500, car.Mileage);
        Assert.Equal(1, _f.Vehicles.SaveCount);
        Assert.Equal(new[] { "INFO|UC-01", "INFO|UC-01" }, _f.Log.Entries);
    }

    [Fact]
    public void UC01_МеншеПоточного_КидаєВиняткоІНеЗмінюєПробіг()
    {
        Vehicle car = _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 1000);

        Assert.Throws<MileageRollbackException>(
            () => _f.MileageService().Register("AA1111BB", "999"));

        Assert.Equal(1000, car.Mileage);
        Assert.Equal(0, _f.Vehicles.SaveCount);
        Assert.Contains("WARN|UC-01", _f.Log.Entries);
    }

    [Fact]
    public void UC01_НевідомийНомер_ПовертаєПомилку()
    {
        Result<Vehicle> result = _f.MileageService().Register("ZZ9999ZZ", "10");

        Assert.False(result.IsSuccess);
        Assert.Contains("не знайдено", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void UC01_НечисловийПробіг_ПовертаєПомилкуБезЗбереження()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 1000);

        Result<Vehicle> result = _f.MileageService().Register("AA1111BB", "abc");

        Assert.False(result.IsSuccess);
        Assert.Equal(0, _f.Vehicles.SaveCount);
    }

    [Fact]
    public void UC02_ПрострочийВан_СтворюєЗамовленняЗНадбавкою()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Van, Recent, 13000);

        Result<ServiceOrder> result = _f.OrderService().Plan("AA1111BB", "OilChange");

        Assert.True(result.IsSuccess);
        Assert.Equal(1725m, result.Value.Cost);
        Assert.Equal(OrderStatus.Planned, result.Value.Status);
        Assert.Single(_f.Orders.Items);
    }

    [Fact]
    public void UC02_СкороПотрібно_ЦінаБезНадбавки()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Van, Recent, 9600);

        Result<ServiceOrder> result = _f.OrderService().Plan("AA1111BB", "OilChange");

        Assert.Equal(1500m, result.Value.Cost);
    }

    [Fact]
    public void UC02_ТОЩеНеПотрібне_Відхиляється()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 500);

        Result<ServiceOrder> result = _f.OrderService().Plan("AA1111BB", "OilChange");

        Assert.False(result.IsSuccess);
        Assert.Empty(_f.Orders.Items);
        Assert.Contains("WARN|UC-02", _f.Log.Entries);
    }

    [Fact]
    public void UC02_ДубльАктивногоЗамовлення_Відхиляється()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 11000);
        ServiceOrderService service = _f.OrderService();
        service.Plan("AA1111BB", "OilChange");

        Result<ServiceOrder> second = service.Plan("AA1111BB", "OilChange");

        Assert.False(second.IsSuccess);
        Assert.Single(_f.Orders.Items);
    }

    [Fact]
    public void UC02_НевідомийВидТО_ПовертаєПомилку()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 11000);

        Result<ServiceOrder> result = _f.OrderService().Plan("AA1111BB", "Wash");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void UC03_Start_ПереводитьВInProgress()
    {
        ServiceOrder order = PlannedOrder();

        Result<ServiceOrder> result = _f.OrderService().Apply("1", "Start");

        Assert.Equal(OrderStatus.InProgress, result.Value.Status);
        Assert.Same(order, result.Value);
    }

    [Fact]
    public void UC03_Complete_ДодаєЗаписТОІСкидаєПотребу()
    {
        Vehicle car = _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 11000);
        ServiceOrderService service = _f.OrderService();
        service.Plan("AA1111BB", "OilChange");
        service.Apply("1", "Start");

        Result<ServiceOrder> done = service.Apply("1", "Complete");

        Assert.Equal(OrderStatus.Done, done.Value.Status);
        Assert.Equal(Fixture.Today, done.Value.CompletedOn);
        ServiceRecord record = Assert.Single(car.History);
        Assert.Equal(11000, record.Point.Mileage);
        MaintenanceCheck check = new MaintenancePolicy()
            .Evaluate(car, ServiceKind.OilChange, Fixture.Today);
        Assert.Equal(MaintenanceStatus.Ok, check.Status);
    }

    [Fact]
    public void UC03_Cancel_ІзЗапланованого_СкасовуєЗамовлення()
    {
        PlannedOrder();

        Result<ServiceOrder> result = _f.OrderService().Apply("1", "Cancel");

        Assert.Equal(OrderStatus.Cancelled, result.Value.Status);
    }

    [Fact]
    public void UC03_ЗабороненийПерехід_КидаєВиняткоІПишеWarning()
    {
        PlannedOrder();

        Assert.Throws<InvalidOrderStateException>(
            () => _f.OrderService().Apply("1", "Complete"));

        Assert.Contains("WARN|UC-03", _f.Log.Entries);
    }

    [Fact]
    public void UC03_НевідомеЗамовлення_ПовертаєПомилку()
    {
        Result<ServiceOrder> result = _f.OrderService().Apply("99", "Start");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void UC06_КоректніДані_ДодаєТЗЗНаступнимНомером()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 10);

        Result<Vehicle> result = _f.VehicleService().Register("kb2222cc", "Ford", "Van", "500");

        Assert.Equal(2, result.Value.Id);
        Assert.Equal("KB2222CC", result.Value.Info.Plate);
        Assert.Equal(Fixture.Today, result.Value.Registered.Date);
        Assert.Equal(2, _f.Vehicles.Items.Count);
    }

    [Fact]
    public void UC06_ДубльНомера_Відхиляється()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 10);

        Result<Vehicle> result = _f.VehicleService().Register("AA1111BB", "Ford", "Van", "500");

        Assert.False(result.IsSuccess);
        Assert.Single(_f.Vehicles.Items);
    }

    [Fact]
    public void UC06_ПорожнійНомер_ПовертаєПершуПомилку()
    {
        Result<Vehicle> result = _f.VehicleService().Register("", "", "Bike", "x");

        Assert.False(result.IsSuccess);
        Assert.Contains("AA1234BB", result.Error, StringComparison.Ordinal);
    }

    private ServiceOrder PlannedOrder()
    {
        _f.AddVehicle("AA1111BB", VehicleType.Car, Recent, 11000);
        _f.OrderService().Plan("AA1111BB", "OilChange");
        return _f.Orders.Items.Single();
    }
}
