using System;

namespace FleetMaintenance.Domain;

/// <summary>Замовлення на обслуговування: документ зі станом.</summary>
public sealed class ServiceOrder
{
    /// <summary>Створює замовлення в стані «заплановано».</summary>
    /// <param name="id">Унікальний номер замовлення.</param>
    /// <param name="vehicleId">Номер ТЗ.</param>
    /// <param name="kind">Вид обслуговування.</param>
    /// <param name="plannedOn">Дата планування.</param>
    public ServiceOrder(int id, int vehicleId, ServiceKind kind, DateOnly plannedOn)
    {
        Id = id;
        VehicleId = vehicleId;
        Kind = kind;
        PlannedOn = plannedOn;
    }

    /// <summary>Унікальний номер замовлення.</summary>
    public int Id { get; }

    /// <summary>Номер ТЗ, до якого належить замовлення.</summary>
    public int VehicleId { get; }

    /// <summary>Вид обслуговування.</summary>
    public ServiceKind Kind { get; }

    /// <summary>Дата планування.</summary>
    public DateOnly PlannedOn { get; }

    /// <summary>Вартість у гривнях, розрахована під час планування.</summary>
    public decimal Cost { get; init; }

    /// <summary>Поточний стан замовлення.</summary>
    public OrderStatus Status { get; private set; } = OrderStatus.Planned;

    /// <summary>Дата завершення; порожня, доки замовлення не виконано.</summary>
    public DateOnly? CompletedOn { get; private set; }

    /// <summary>Чи може замовлення ще змінюватися.</summary>
    public bool IsActive => Status is OrderStatus.Planned or OrderStatus.InProgress;

    /// <summary>Відновлює стан зі сховища; не перевіряє переходів.</summary>
    /// <param name="status">Збережений стан.</param>
    /// <param name="completedOn">Збережена дата завершення.</param>
    public void Restore(OrderStatus status, DateOnly? completedOn)
    {
        Status = status;
        CompletedOn = completedOn;
    }

    /// <summary>Починає виконання.</summary>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public void Start()
    {
        Status = OrderTransitions.Next(Status, OrderEvent.Start);
    }

    /// <summary>Завершує виконання.</summary>
    /// <param name="on">Дата завершення.</param>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public void Complete(DateOnly on)
    {
        Status = OrderTransitions.Next(Status, OrderEvent.Complete);
        CompletedOn = on;
    }

    /// <summary>Скасовує замовлення.</summary>
    /// <exception cref="InvalidOrderStateException">Перехід заборонено.</exception>
    public void Cancel()
    {
        Status = OrderTransitions.Next(Status, OrderEvent.Cancel);
    }
}
