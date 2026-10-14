using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>UC-01 «Зареєструвати пробіг».</summary>
public sealed class MileageService
{
    private readonly IVehicleRepository _vehicles;
    private readonly AppEnvironment _env;

    /// <summary>Створює сервіс.</summary>
    /// <param name="vehicles">Сховище ТЗ.</param>
    /// <param name="env">Годинник і журнал.</param>
    public MileageService(IVehicleRepository vehicles, AppEnvironment env)
    {
        _vehicles = vehicles;
        _env = env;
    }

    /// <summary>Фіксує новий показник одометра ТЗ.</summary>
    /// <param name="plate">Державний номер.</param>
    /// <param name="mileage">Новий пробіг, км.</param>
    /// <returns>Оновлене ТЗ або текст помилки введення.</returns>
    /// <exception cref="MileageRollbackException">Пробіг менший за поточний.</exception>
    public Result<Vehicle> Register(string? plate, string? mileage)
    {
        _env.Log.Information("UC-01", "початок");
        Result<Vehicle> found = VehicleLookup.Find(_vehicles, plate);
        Result<int> km = InputValidator.Mileage(mileage);
        string? error = Result.FirstError(found, km);
        if (error is not null)
        {
            _env.Log.Warning("UC-01", error);
            return Result<Vehicle>.Fail(error);
        }

        Vehicle vehicle = found.Value;
        Apply(vehicle, km.Value);
        _vehicles.Save();
        _env.Log.Information("UC-01", $"ТЗ №{vehicle.Id}: пробіг {km.Value} км");
        return Result<Vehicle>.Ok(vehicle);
    }

    private void Apply(Vehicle vehicle, int mileage)
    {
        try
        {
            vehicle.RegisterMileage(mileage);
        }
        catch (MileageRollbackException ex)
        {
            _env.Log.Warning("UC-01", ex.Message);
            throw;
        }
    }
}
