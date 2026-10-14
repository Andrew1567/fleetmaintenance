using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>Пошук ТЗ за введеним номером: перевірка формату й наявності.</summary>
internal static class VehicleLookup
{
    internal static Result<Vehicle> Find(IVehicleRepository vehicles, string? plate)
    {
        Result<string> p = InputValidator.Plate(plate);
        if (!p.IsSuccess)
        {
            return Result<Vehicle>.Fail(p.Error!);
        }

        Vehicle? vehicle = vehicles.FindByPlate(p.Value);
        return vehicle is null
            ? Result<Vehicle>.Fail($"ТЗ {p.Value} не знайдено.")
            : Result<Vehicle>.Ok(vehicle);
    }
}
