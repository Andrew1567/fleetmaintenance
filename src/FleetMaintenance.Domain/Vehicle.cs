using System;
using System.Collections.Generic;
using System.Linq;

namespace FleetMaintenance.Domain;

/// <summary>Транспортний засіб автопарку з пробігом й історією ТО.</summary>
public sealed class Vehicle
{
    private readonly List<ServiceRecord> _history = new();

    /// <summary>Створює ТЗ на момент реєстрації в системі.</summary>
    /// <param name="id">Унікальний номер у системі.</param>
    /// <param name="info">Паспортні дані.</param>
    /// <param name="registered">Дата реєстрації та пробіг на цю дату.</param>
    public Vehicle(int id, VehicleInfo info, ServicePoint registered)
    {
        Id = id;
        Info = info;
        Registered = registered;
        Mileage = registered.Mileage;
    }

    /// <summary>Унікальний номер у системі.</summary>
    public int Id { get; }

    /// <summary>Паспортні дані.</summary>
    public VehicleInfo Info { get; }

    /// <summary>Дата реєстрації й пробіг на цю дату.</summary>
    public ServicePoint Registered { get; }

    /// <summary>Поточний пробіг, км.</summary>
    public int Mileage { get; private set; }

    /// <summary>Історія виконаних обслуговувань.</summary>
    public IReadOnlyList<ServiceRecord> History => _history;

    /// <summary>Фіксує новий показник одометра.</summary>
    /// <param name="newMileage">Новий пробіг, км; не менший за поточний.</param>
    /// <exception cref="MileageRollbackException">Новий пробіг менший за поточний.</exception>
    public void RegisterMileage(int newMileage)
    {
        if (newMileage < Mileage)
        {
            throw new MileageRollbackException(Mileage, newMileage);
        }

        Mileage = newMileage;
    }

    /// <summary>Додає запис про виконане ТО.</summary>
    /// <param name="record">Запис; пробіг у ньому не більший за поточний.</param>
    /// <exception cref="ArgumentException">Пробіг запису більший за поточний.</exception>
    public void AddServiceRecord(ServiceRecord record)
    {
        if (record.Point.Mileage > Mileage)
        {
            throw new ArgumentException("Пробіг ТО більший за поточний.", nameof(record));
        }

        _history.Add(record);
    }

    /// <summary>Знаходить точку відліку для інтервалу заданого виду ТО.</summary>
    /// <param name="kind">Вид обслуговування.</param>
    /// <returns>Останнє ТО цього виду, а за його відсутності — реєстрацію.</returns>
    public ServicePoint LastServiceOrRegistration(ServiceKind kind)
    {
        ServiceRecord? last = _history
            .Where(r => r.Kind == kind)
            .OrderByDescending(r => r.Point.Date)
            .FirstOrDefault();
        return last is null ? Registered : last.Point;
    }
}
