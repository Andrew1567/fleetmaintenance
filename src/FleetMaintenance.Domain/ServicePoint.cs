using System;

namespace FleetMaintenance.Domain;

/// <summary>Точка відліку інтервалу: дата й пробіг.</summary>
/// <param name="Date">Дата реєстрації ТЗ або виконаного ТО.</param>
/// <param name="Mileage">Пробіг у кілометрах у цей момент.</param>
public sealed record ServicePoint(DateOnly Date, int Mileage);

/// <summary>Паспортні дані транспортного засобу.</summary>
/// <param name="Plate">Державний номер у форматі AA1234BB.</param>
/// <param name="Model">Марка й модель.</param>
/// <param name="Type">Тип ТЗ.</param>
public sealed record VehicleInfo(string Plate, string Model, VehicleType Type);

/// <summary>Запис про виконане обслуговування.</summary>
/// <param name="Kind">Вид обслуговування.</param>
/// <param name="Point">Дата й пробіг, коли ТО виконано.</param>
public sealed record ServiceRecord(ServiceKind Kind, ServicePoint Point);
