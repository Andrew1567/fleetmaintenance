using System;
using System.Collections.Generic;

namespace FleetMaintenance.Infrastructure;

/// <summary>Серіалізаційна форма запису про ТО.</summary>
public sealed class ServiceRecordDto
{
    /// <summary>Вид обслуговування (ім'я перелічення).</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Дата виконання.</summary>
    public DateOnly Date { get; set; }

    /// <summary>Пробіг на момент ТО, км.</summary>
    public int Mileage { get; set; }
}

/// <summary>Серіалізаційна форма транспортного засобу.</summary>
public sealed class VehicleDto
{
    /// <summary>Номер у системі.</summary>
    public int Id { get; set; }

    /// <summary>Державний номер.</summary>
    public string Plate { get; set; } = string.Empty;

    /// <summary>Марка й модель.</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Тип ТЗ (ім'я перелічення).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Дата реєстрації.</summary>
    public DateOnly RegisteredOn { get; set; }

    /// <summary>Пробіг на дату реєстрації, км.</summary>
    public int RegisteredMileage { get; set; }

    /// <summary>Поточний пробіг, км.</summary>
    public int Mileage { get; set; }

    /// <summary>Історія обслуговувань.</summary>
    public List<ServiceRecordDto> History { get; set; } = new();
}

/// <summary>Серіалізаційна форма замовлення.</summary>
public sealed class OrderDto
{
    /// <summary>Номер замовлення.</summary>
    public int Id { get; set; }

    /// <summary>Номер ТЗ.</summary>
    public int VehicleId { get; set; }

    /// <summary>Вид ТО (ім'я перелічення).</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Стан (ім'я перелічення).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Дата планування.</summary>
    public DateOnly PlannedOn { get; set; }

    /// <summary>Дата завершення.</summary>
    public DateOnly? CompletedOn { get; set; }

    /// <summary>Вартість, грн.</summary>
    public decimal Cost { get; set; }
}
