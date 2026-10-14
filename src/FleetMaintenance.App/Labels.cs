using System.Globalization;
using FleetMaintenance.Domain;

namespace FleetMaintenance.App;

/// <summary>Українські підписи перелічень для виводу.</summary>
internal static class Labels
{
    internal static string Of(MaintenanceStatus status)
    {
        return status switch
        {
            MaintenanceStatus.Ok => "норма",
            MaintenanceStatus.DueSoon => "скоро",
            _ => "прострочено",
        };
    }

    internal static string Of(ServiceKind kind)
    {
        return kind switch
        {
            ServiceKind.OilChange => "мастило",
            ServiceKind.Brakes => "гальма",
            _ => "огляд",
        };
    }

    internal static string Of(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Planned => "заплановано",
            OrderStatus.InProgress => "виконується",
            OrderStatus.Done => "виконано",
            _ => "скасовано",
        };
    }

    internal static string Money(decimal value)
    {
        return value.ToString("F2", CultureInfo.InvariantCulture);
    }
}
