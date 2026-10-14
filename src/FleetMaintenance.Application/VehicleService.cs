using System;
using System.Collections.Generic;
using System.Linq;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>UC-06 «Додати ТЗ» і UC-05 «Знайти ТЗ».</summary>
public sealed class VehicleService
{
    private readonly IVehicleRepository _vehicles;
    private readonly AppEnvironment _env;

    /// <summary>Створює сервіс.</summary>
    /// <param name="vehicles">Сховище ТЗ.</param>
    /// <param name="env">Годинник і журнал.</param>
    public VehicleService(IVehicleRepository vehicles, AppEnvironment env)
    {
        _vehicles = vehicles;
        _env = env;
    }

    /// <summary>UC-06. Реєструє нове ТЗ у системі.</summary>
    /// <param name="plate">Державний номер у форматі AA1234BB.</param>
    /// <param name="model">Марка й модель.</param>
    /// <param name="type">Тип ТЗ: Car, Van або Truck.</param>
    /// <param name="mileage">Поточний пробіг, км.</param>
    /// <returns>Створене ТЗ або текст помилки введення.</returns>
    public Result<Vehicle> Register(string? plate, string? model, string? type, string? mileage)
    {
        Result<string> p = InputValidator.Plate(plate);
        Result<string> m = InputValidator.Model(model);
        Result<VehicleType> t = InputValidator.Type(type);
        Result<int> km = InputValidator.Mileage(mileage);
        string? error = Result.FirstError(p, m, t, km);
        if (error is null && _vehicles.FindByPlate(p.Value) is not null)
        {
            error = $"ТЗ з номером {p.Value} уже зареєстровано.";
        }

        if (error is not null)
        {
            _env.Log.Warning("UC-06", error);
            return Result<Vehicle>.Fail(error);
        }

        var info = new VehicleInfo(p.Value, m.Value, t.Value);
        var registered = new ServicePoint(_env.Today, km.Value);
        var vehicle = new Vehicle(_vehicles.NextId(), info, registered);
        _vehicles.Add(vehicle);
        _vehicles.Save();
        _env.Log.Information("UC-06", $"додано ТЗ №{vehicle.Id}");
        return Result<Vehicle>.Ok(vehicle);
    }

    /// <summary>UC-05. Шукає ТЗ за текстом і/або типом.</summary>
    /// <param name="text">Частина номера чи моделі; порожній рядок — без фільтра.</param>
    /// <param name="type">Тип ТЗ; порожній рядок — без фільтра.</param>
    /// <returns>ТЗ, відсортовані за номером, або помилка введення типу.</returns>
    public Result<IReadOnlyList<Vehicle>> Search(string? text, string? type)
    {
        bool anyType = !string.IsNullOrWhiteSpace(type);
        Result<VehicleType> t = InputValidator.Type(type);
        if (anyType && !t.IsSuccess)
        {
            return Result<IReadOnlyList<Vehicle>>.Fail(t.Error!);
        }

        IEnumerable<Vehicle> query = _vehicles.GetAll();
        if (!string.IsNullOrWhiteSpace(text))
        {
            string part = text.Trim();
            query = query.Where(v => Matches(v.Info, part));
        }

        if (anyType)
        {
            query = query.Where(v => v.Info.Type == t.Value);
        }

        IReadOnlyList<Vehicle> found = query
            .OrderBy(v => v.Info.Plate, StringComparer.Ordinal)
            .ToList();
        return Result<IReadOnlyList<Vehicle>>.Ok(found);
    }

    private static bool Matches(VehicleInfo info, string part)
    {
        return info.Plate.Contains(part, StringComparison.OrdinalIgnoreCase)
            || info.Model.Contains(part, StringComparison.OrdinalIgnoreCase);
    }
}
