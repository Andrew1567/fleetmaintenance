using System;
using System.Collections.Generic;
using System.Linq;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

/// <summary>Рядок звіту: ТЗ і вид ТО, який потрібен.</summary>
/// <param name="Plate">Державний номер.</param>
/// <param name="Model">Марка й модель.</param>
/// <param name="Check">Стан за пробігом і датою.</param>
/// <param name="Cost">Орієнтовна вартість, грн.</param>
public sealed record DueVehicleLine(
    string Plate,
    string Model,
    MaintenanceCheck Check,
    decimal Cost);

/// <summary>UC-04 «Звіт: машини, що потребують ТО».</summary>
public sealed class ReportService
{
    private readonly IVehicleRepository _vehicles;
    private readonly ServiceAdvisor _advisor;
    private readonly AppEnvironment _env;

    /// <summary>Створює сервіс.</summary>
    /// <param name="vehicles">Сховище ТЗ.</param>
    /// <param name="advisor">Оцінка потреби в ТО й вартості.</param>
    /// <param name="env">Годинник і журнал.</param>
    public ReportService(IVehicleRepository vehicles, ServiceAdvisor advisor, AppEnvironment env)
    {
        _vehicles = vehicles;
        _advisor = advisor;
        _env = env;
    }

    /// <summary>Дата, станом на яку будується звіт.</summary>
    public DateOnly AsOf => _env.Today;

    /// <summary>Будує перелік ТЗ, яким потрібне ТО, найтерміновіші першими.</summary>
    /// <returns>Рядки за спаданням терміновості, потім за номером і видом ТО.</returns>
    public IReadOnlyList<DueVehicleLine> DueVehicles()
    {
        _env.Log.Information("UC-04", "початок");
        DateOnly today = _env.Today;
        List<DueVehicleLine> lines = _vehicles.GetAll()
            .SelectMany(v => LinesFor(v, today))
            .OrderByDescending(l => l.Check.Status)
            .ThenBy(l => l.Plate, StringComparer.Ordinal)
            .ThenBy(l => l.Check.Kind)
            .ToList();
        _env.Log.Information("UC-04", $"рядків у звіті: {lines.Count}");
        return lines;
    }

    /// <summary>Підсумовує вартість рядків звіту.</summary>
    /// <param name="lines">Рядки звіту.</param>
    /// <returns>Сума в гривнях.</returns>
    public static decimal TotalCost(IEnumerable<DueVehicleLine> lines)
    {
        return lines.Sum(l => l.Cost);
    }

    private IEnumerable<DueVehicleLine> LinesFor(Vehicle vehicle, DateOnly today)
    {
        return Enum.GetValues<ServiceKind>()
            .Select(kind => _advisor.Assess(vehicle, kind, today))
            .Where(a => a.Check.Status != MaintenanceStatus.Ok)
            .Select(a => new DueVehicleLine(
                vehicle.Info.Plate,
                vehicle.Info.Model,
                a.Check,
                a.Cost));
    }
}
