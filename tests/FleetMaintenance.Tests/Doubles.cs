using System;
using System.Collections.Generic;
using System.Linq;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;
using FleetMaintenance.Infrastructure;

namespace FleetMaintenance.Tests;

/// <summary>Фейкове сховище ТЗ у пам'яті.</summary>
internal sealed class FakeVehicleRepository : IVehicleRepository
{
    internal List<Vehicle> Items { get; } = new();

    internal int SaveCount { get; private set; }

    public IReadOnlyList<Vehicle> GetAll() => Items;

    public Vehicle? GetById(int id) => Items.FirstOrDefault(v => v.Id == id);

    public Vehicle? FindByPlate(string plate) => Items.FirstOrDefault(v => v.Info.Plate == plate);

    public void Add(Vehicle vehicle) => Items.Add(vehicle);

    public int NextId() => Items.Count + 1;

    public void Save() => SaveCount++;
}

/// <summary>Фейкове сховище замовлень у пам'яті.</summary>
internal sealed class FakeServiceOrderRepository : IServiceOrderRepository
{
    internal List<ServiceOrder> Items { get; } = new();

    public IReadOnlyList<ServiceOrder> GetAll() => Items;

    public ServiceOrder? GetById(int id) => Items.FirstOrDefault(o => o.Id == id);

    public void Add(ServiceOrder order) => Items.Add(order);

    public int NextId() => Items.Count + 1;

    public void Save()
    {
    }
}

/// <summary>Шпигун: запам'ятовує записи журналу для перевірки.</summary>
internal sealed class RecordingLogger : IAppLogger
{
    internal List<string> Entries { get; } = new();

    public void Information(string scenario, string message) => Entries.Add($"INFO|{scenario}");

    public void Warning(string scenario, string message) => Entries.Add($"WARN|{scenario}");

    public void Error(string scenario, string message, Exception error) =>
        Entries.Add($"ERROR|{scenario}");
}

/// <summary>Спільне оточення тестів: фіксована дата, фейкові сховища, служби.</summary>
internal sealed class Fixture
{
    internal static readonly DateOnly Today = new(2026, 11, 2);

    internal Fixture()
    {
        Env = new AppEnvironment(new FixedTimeProvider(Today), Log);
        Advisor = new ServiceAdvisor(new MaintenancePolicy(), new PricingPolicy());
    }

    internal FakeVehicleRepository Vehicles { get; } = new();

    internal FakeServiceOrderRepository Orders { get; } = new();

    internal RecordingLogger Log { get; } = new();

    internal AppEnvironment Env { get; }

    internal ServiceAdvisor Advisor { get; }

    internal VehicleService VehicleService() => new(Vehicles, Env);

    internal MileageService MileageService() => new(Vehicles, Env);

    internal ServiceOrderService OrderService() => new(Vehicles, Orders, Advisor, Env);

    internal ReportService ReportService() => new(Vehicles, Advisor, Env);

    internal Vehicle AddVehicle(string plate, VehicleType type, DateOnly registeredOn, int mileage)
    {
        var info = new VehicleInfo(plate, "Test", type);
        Vehicle vehicle = Make(Vehicles.NextId(), info, registeredOn, mileage);
        Vehicles.Add(vehicle);
        return vehicle;
    }

    internal static Vehicle Make(int id, VehicleInfo info, DateOnly on, int mileage)
    {
        var vehicle = new Vehicle(id, info, new ServicePoint(on, 0));
        vehicle.RegisterMileage(mileage);
        return vehicle;
    }
}
