using System;
using System.Linq;
using FleetMaintenance.Domain;

namespace FleetMaintenance.Application;

// TODO TD-03: сервіс має дві відповідальності (UC-02 і UC-03); розділити (1,5 год).
/// <summary>UC-02 «Запланувати обслуговування» і UC-03 «Змінити стан замовлення».</summary>
public sealed class ServiceOrderService
{
    private readonly IVehicleRepository _vehicles;
    private readonly IServiceOrderRepository _orders;
    private readonly ServiceAdvisor _advisor;
    private readonly AppEnvironment _env;

    /// <summary>Створює сервіс.</summary>
    /// <param name="vehicles">Сховище ТЗ.</param>
    /// <param name="orders">Сховище замовлень.</param>
    /// <param name="advisor">Оцінка потреби в ТО й вартості.</param>
    /// <param name="env">Годинник і журнал.</param>
    public ServiceOrderService(
        IVehicleRepository vehicles,
        IServiceOrderRepository orders,
        ServiceAdvisor advisor,
        AppEnvironment env)
    {
        _vehicles = vehicles;
        _orders = orders;
        _advisor = advisor;
        _env = env;
    }

    /// <summary>UC-02. Планує обслуговування, якщо воно потрібне.</summary>
    /// <param name="plate">Державний номер.</param>
    /// <param name="kind">Вид ТО: OilChange, Brakes або Inspection.</param>
    /// <returns>Нове замовлення або причина відмови.</returns>
    public Result<ServiceOrder> Plan(string? plate, string? kind)
    {
        _env.Log.Information("UC-02", "початок");
        Result<Vehicle> found = VehicleLookup.Find(_vehicles, plate);
        Result<ServiceKind> k = InputValidator.Kind(kind);
        string? error = Result.FirstError(found, k);
        if (error is not null)
        {
            return Reject("UC-02", error);
        }

        Vehicle vehicle = found.Value;
        if (HasActiveOrder(vehicle.Id, k.Value))
        {
            return Reject("UC-02", "Для цього ТЗ уже є активне замовлення такого виду.");
        }

        ServiceAssessment assessment = _advisor.Assess(vehicle, k.Value, _env.Today);
        if (assessment.Check.Status == MaintenanceStatus.Ok)
        {
            return Reject("UC-02", "Обслуговування ще не потрібне.");
        }

        var order = new ServiceOrder(_orders.NextId(), vehicle.Id, k.Value, _env.Today) {
            Cost = assessment.Cost,
        };
        _orders.Add(order);
        _orders.Save();
        _env.Log.Information("UC-02", $"замовлення №{order.Id} для ТЗ №{vehicle.Id}");
        return Result<ServiceOrder>.Ok(order);
    }

    /// <summary>UC-03. Змінює стан замовлення за таблицею переходів.</summary>
    /// <param name="orderId">Номер замовлення.</param>
    /// <param name="trigger">Подія: Start, Complete або Cancel.</param>
    /// <returns>Оновлене замовлення або текст помилки введення.</returns>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public Result<ServiceOrder> Apply(string? orderId, string? trigger)
    {
        _env.Log.Information("UC-03", "початок");
        Result<int> id = InputValidator.OrderId(orderId);
        Result<OrderEvent> ev = InputValidator.Trigger(trigger);
        string? error = Result.FirstError(id, ev);
        ServiceOrder? order = error is null ? _orders.GetById(id.Value) : null;
        if (error is null && order is null)
        {
            error = $"Замовлення №{id.Value} не знайдено.";
        }

        if (error is not null)
        {
            return Reject("UC-03", error);
        }

        Transition(order!, ev.Value);
        _orders.Save();
        _env.Log.Information("UC-03", $"замовлення №{order!.Id}: {order.Status}");
        return Result<ServiceOrder>.Ok(order);
    }

    private bool HasActiveOrder(int vehicleId, ServiceKind kind)
    {
        return _orders.GetAll()
            .Any(o => o.IsActive && o.VehicleId == vehicleId && o.Kind == kind);
    }

    private void Transition(ServiceOrder order, OrderEvent trigger)
    {
        try
        {
            ApplyEvent(order, trigger);
        }
        catch (InvalidOrderStateException ex)
        {
            _env.Log.Warning("UC-03", ex.Message);
            throw;
        }
    }

    private void ApplyEvent(ServiceOrder order, OrderEvent trigger)
    {
        switch (trigger)
        {
            case OrderEvent.Start:
                order.Start();
                break;
            case OrderEvent.Cancel:
                order.Cancel();
                break;
            case OrderEvent.Complete:
                Complete(order);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(trigger));
        }
    }

    private void Complete(ServiceOrder order)
    {
        Vehicle vehicle = _vehicles.GetById(order.VehicleId)
            ?? throw new InvalidOperationException($"Немає ТЗ №{order.VehicleId}.");
        order.Complete(_env.Today);
        var point = new ServicePoint(_env.Today, vehicle.Mileage);
        vehicle.AddServiceRecord(new ServiceRecord(order.Kind, point));
        _vehicles.Save();
    }

    private Result<ServiceOrder> Reject(string scenario, string message)
    {
        _env.Log.Warning(scenario, message);
        return Result<ServiceOrder>.Fail(message);
    }
}
