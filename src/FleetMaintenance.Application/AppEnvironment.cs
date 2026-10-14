using System;

namespace FleetMaintenance.Application;

/// <summary>Середовище виконання сценаріїв: годинник і журнал.</summary>
/// <param name="Time">Джерело часу; у тестах підміняється фіксованим.</param>
/// <param name="Log">Журнал подій.</param>
public sealed record AppEnvironment(TimeProvider Time, IAppLogger Log)
{
    /// <summary>Поточна локальна дата.</summary>
    public DateOnly Today => DateOnly.FromDateTime(Time.GetLocalNow().DateTime);
}
