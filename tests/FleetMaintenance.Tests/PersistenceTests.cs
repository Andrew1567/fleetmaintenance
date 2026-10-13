using System;
using System.IO;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using FleetMaintenance.Infrastructure;
using Xunit;

namespace FleetMaintenance.Tests;

public sealed class PersistenceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    [Fact]
    public void UC07_ТЗЗІсторією_ВідновлюєтьсяПісляЗбереження()
    {
        string path = Path.Combine(_dir, "vehicles.json");
        var repo = new JsonVehicleRepository(path);
        var info = new VehicleInfo("AA1111BB", "Test", VehicleType.Van);
        Vehicle car = Fixture.Make(1, info, Fixture.Today, 500);
        var point = new ServicePoint(Fixture.Today, 400);
        car.AddServiceRecord(new ServiceRecord(ServiceKind.Brakes, point));
        repo.Add(car);
        repo.Save();

        var loaded = new JsonVehicleRepository(path);

        Vehicle restored = Assert.Single(loaded.GetAll());
        Assert.Equal("AA1111BB", restored.Info.Plate);
        Assert.Equal(VehicleType.Van, restored.Info.Type);
        Assert.Equal(500, restored.Mileage);
        Assert.Equal(400, Assert.Single(restored.History).Point.Mileage);
        Assert.Equal(2, loaded.NextId());
    }

    [Fact]
    public void UC07_Замовлення_ЗберігаєСтанІВартість()
    {
        string path = Path.Combine(_dir, "orders.json");
        var repo = new JsonServiceOrderRepository(path);
        var order = new ServiceOrder(7, 1, ServiceKind.Inspection, Fixture.Today) { Cost = 1725m };
        order.Start();
        repo.Add(order);
        repo.Save();

        var loaded = new JsonServiceOrderRepository(path);

        ServiceOrder restored = Assert.Single(loaded.GetAll());
        Assert.Equal(OrderStatus.InProgress, restored.Status);
        Assert.Equal(1725m, restored.Cost);
        Assert.Equal(8, loaded.NextId());
    }

    [Fact]
    public void UC07_ВідсутнійФайл_ДаєПорожнєСховище()
    {
        var repo = new JsonVehicleRepository(Path.Combine(_dir, "none.json"));

        Assert.Empty(repo.GetAll());
        Assert.Equal(1, repo.NextId());
    }

    [Fact]
    public void UC07_ПошкодженийФайл_КидаєInvalidDataException()
    {
        Directory.CreateDirectory(_dir);
        string path = Path.Combine(_dir, "vehicles.json");
        File.WriteAllText(path, "{ not json");

        Assert.Throws<InvalidDataException>(() => new JsonVehicleRepository(path));
    }

    [Fact]
    public void FileLogger_ЗаписуєЧасРівеньСценарійІПовідомлення()
    {
        string path = Path.Combine(_dir, "log.txt");
        var logger = new FileLogger(path, new FixedTimeProvider(Fixture.Today));

        logger.Warning("UC-02", "відхилено");

        string line = File.ReadAllText(path).Trim();
        Assert.Equal("2026-11-02 00:00:00|WARN|UC-02|відхилено", line);
    }
}
