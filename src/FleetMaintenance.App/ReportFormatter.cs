using System;
using System.Collections.Generic;
using FleetMaintenance.Application;

namespace FleetMaintenance.App;

/// <summary>Форматує звіт «Машини, що потребують ТО» для консолі.</summary>
internal static class ReportFormatter
{
    internal static IEnumerable<string> Format(IReadOnlyList<DueVehicleLine> lines, DateOnly asOf)
    {
        yield return $"Машини, що потребують ТО, станом на {asOf:yyyy-MM-dd}";
        yield return $"{"Номер",-10}{"Модель",-22}{"ТО",-9}{"Стан",-12}{"км",7}{"міс",5}{"грн",11}";
        foreach (DueVehicleLine line in lines)
        {
            yield return Row(line);
        }

        decimal total = ReportService.TotalCost(lines);
        yield return $"Разом: {lines.Count} рядків, {Labels.Money(total)} грн";
    }

    private static string Row(DueVehicleLine line)
    {
        MaintenanceCheck c = line.Check;
        string cost = Labels.Money(line.Cost);
        return $"{line.Plate,-10}{line.Model,-22}{Labels.Of(c.Kind),-9}"
            + $"{Labels.Of(c.Status),-12}{c.KmSince,7}{c.MonthsSince,5}{cost,11}";
    }
}
