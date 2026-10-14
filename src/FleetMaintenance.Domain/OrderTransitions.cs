using System.Collections.Generic;

namespace FleetMaintenance.Domain;

/// <summary>Таблиця переходів станів замовлення (розділ 1.5 специфікації).</summary>
public static class OrderTransitions
{
    private static readonly Dictionary<(OrderStatus, OrderEvent), OrderStatus> Table = new() {
        [(OrderStatus.Planned, OrderEvent.Start)] = OrderStatus.InProgress,
        [(OrderStatus.Planned, OrderEvent.Cancel)] = OrderStatus.Cancelled,
        [(OrderStatus.InProgress, OrderEvent.Complete)] = OrderStatus.Done,
        [(OrderStatus.InProgress, OrderEvent.Cancel)] = OrderStatus.Cancelled,
    };

    /// <summary>Визначає стан після події.</summary>
    /// <param name="origin">Поточний стан.</param>
    /// <param name="trigger">Подія.</param>
    /// <returns>Новий стан.</returns>
    /// <exception cref="InvalidOrderStateException">Пара «стан — подія» заборонена.</exception>
    public static OrderStatus Next(OrderStatus origin, OrderEvent trigger)
    {
        if (!Table.TryGetValue((origin, trigger), out OrderStatus next))
        {
            throw new InvalidOrderStateException(origin, trigger);
        }

        return next;
    }
}
