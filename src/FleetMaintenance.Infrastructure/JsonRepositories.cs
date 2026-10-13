using System.Collections.Generic;
using System.Linq;
using FleetMaintenance.Application;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Infrastructure;

// TODO TD-02: два репозиторії дублюють код; виділити JsonRepository<T> (2 год).
/// <summary>Сховище ТЗ у файлі JSON (UC-07).</summary>
public sealed class JsonVehicleRepository : IVehicleRepository
{
    private readonly string _path;
    private readonly List<Vehicle> _items;

    /// <summary>Завантажує дані з файлу; відсутній файл дає порожнє сховище.</summary>
    /// <param name="path">Шлях до файлу JSON.</param>
    public JsonVehicleRepository(string path)
    {
        _path = path;
        _items = JsonFile.Read<VehicleDto>(path).Select(Mappers.ToVehicle).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<Vehicle> GetAll()
    {
        return _items;
    }

    /// <inheritdoc />
    public Vehicle? GetById(int id)
    {
        return _items.FirstOrDefault(v => v.Id == id);
    }

    /// <inheritdoc />
    public Vehicle? FindByPlate(string plate)
    {
        return _items.FirstOrDefault(v => v.Info.Plate == plate);
    }

    /// <inheritdoc />
    public void Add(Vehicle vehicle)
    {
        _items.Add(vehicle);
    }

    /// <inheritdoc />
    public int NextId()
    {
        return _items.Count == 0 ? 1 : _items.Max(v => v.Id) + 1;
    }

    /// <inheritdoc />
    public void Save()
    {
        JsonFile.Write(_path, _items.Select(Mappers.ToVehicleDto));
    }
}

/// <summary>Сховище замовлень у файлі JSON (UC-07).</summary>
public sealed class JsonServiceOrderRepository : IServiceOrderRepository
{
    private readonly string _path;
    private readonly List<ServiceOrder> _items;

    /// <summary>Завантажує дані з файлу; відсутній файл дає порожнє сховище.</summary>
    /// <param name="path">Шлях до файлу JSON.</param>
    public JsonServiceOrderRepository(string path)
    {
        _path = path;
        _items = JsonFile.Read<OrderDto>(path).Select(Mappers.ToOrder).ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<ServiceOrder> GetAll()
    {
        return _items;
    }

    /// <inheritdoc />
    public ServiceOrder? GetById(int id)
    {
        return _items.FirstOrDefault(o => o.Id == id);
    }

    /// <inheritdoc />
    public void Add(ServiceOrder order)
    {
        _items.Add(order);
    }

    /// <inheritdoc />
    public int NextId()
    {
        return _items.Count == 0 ? 1 : _items.Max(o => o.Id) + 1;
    }

    /// <inheritdoc />
    public void Save()
    {
        JsonFile.Write(_path, _items.Select(Mappers.ToOrderDto));
    }
}
